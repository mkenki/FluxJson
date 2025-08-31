// src/FluxJson.Core/JsonWriter.cs - Complete Implementation
using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace FluxJson.Core;

/// <summary>
/// A high-performance JSON writer that writes JSON data to a provided buffer.
/// </summary>
public ref struct JsonWriter
{
    private Span<byte> _buffer;
    private int _position;
    private readonly JsonConfiguration _config;
    private int _indentLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWriter"/> struct.
    /// </summary>
    /// <param name="buffer">The buffer to write JSON to.</param>
    /// <param name="config">The JSON configuration settings.</param>
    public JsonWriter(Span<byte> buffer, JsonConfiguration config)
    {
        _buffer = buffer;
        _position = 0;
        _config = config;
        _indentLevel = 0;
    }

    /// <summary>
    /// Gets the current position in the buffer.
    /// </summary>
    public readonly int Position => _position;
    /// <summary>
    /// Gets the remaining space in the buffer.
    /// </summary>
    public readonly int Remaining => _buffer.Length - _position;

    /// <summary>
    /// Gets the span of bytes that have been written to the buffer.
    /// </summary>
    public readonly ReadOnlySpan<byte> WrittenSpan => _buffer.Slice(0, _position);

    /// <summary>
    /// Writes the start of a JSON object ('{').
    /// </summary>
    public void WriteStartObject()
    {
        WriteByte((byte)'{');
        if (_config.WriteIndented)
        {
            _indentLevel++;
        }
    }

    /// <summary>
    /// Writes the end of a JSON object ('}').
    /// </summary>
    public void WriteEndObject()
    {
        if (_config.WriteIndented)
        {
            _indentLevel--;
            WriteNewLineAndIndent();
        }
        WriteByte((byte)'}');
    }

    /// <summary>
    /// Writes the start of a JSON array ('[').
    /// </summary>
    public void WriteStartArray()
    {
        WriteByte((byte)'[');
        if (_config.WriteIndented)
        {
            _indentLevel++;
        }
    }

    /// <summary>
    /// Writes the end of a JSON array (']').
    /// </summary>
    public void WriteEndArray()
    {
        if (_config.WriteIndented)
        {
            _indentLevel--;
            WriteNewLineAndIndent();
        }
        WriteByte((byte)']');
    }

    /// <summary>
    /// Writes a JSON property name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public void WritePropertyName(ReadOnlySpan<char> name)
    {
        if (_config.WriteIndented)
        {
            WriteNewLineAndIndent();
        }

        WriteByte((byte)'"');
        WriteStringEscaped(name);
        WriteByte((byte)'"');
        WriteByte((byte)':');

        if (_config.WriteIndented)
            WriteByte((byte)' ');
    }

    /// <summary>
    /// Writes a JSON property name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    public void WritePropertyName(string name)
    {
        WritePropertyName(name.AsSpan());
    }

    /// <summary>
    /// Writes a string value to the buffer without escaping.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteString(ReadOnlySpan<char> value)
    {
        var maxByteCount = _config.Encoding.GetMaxByteCount(value.Length);
        if (maxByteCount > Remaining)
            throw new InvalidOperationException($"Buffer overflow at position {_position}");

        var bytesWritten = _config.Encoding.GetBytes(value, _buffer[_position..]);
        _position += bytesWritten;
    }

    /// <summary>
    /// Writes a string value to the buffer, escaping special characters.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteStringEscaped(ReadOnlySpan<char> value)
    {
        int currentSegmentStart = 0;
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            ReadOnlySpan<byte> escapeSequence = default;

            switch (c)
            {
                case '"':
                    escapeSequence = "\\\""u8;
                    break;
                case '\\':
                    escapeSequence = "\\\\"u8;
                    break;
                case '\b':
                    escapeSequence = "\\b"u8;
                    break;
                case '\f':
                    escapeSequence = "\\f"u8;
                    break;
                case '\n':
                    escapeSequence = "\\n"u8;
                    break;
                case '\r':
                    escapeSequence = "\\r"u8;
                    break;
                case '\t':
                    escapeSequence = "\\t"u8;
                    break;
                default:
                    if (c < 0x20) // Control characters
                    {
                        // Write any pending non-escaped segment
                        if (i > currentSegmentStart)
                        {
                            WriteString(value.Slice(currentSegmentStart, i - currentSegmentStart));
                        }

                        WriteBytes("\\u"u8);
                        WriteHex((byte)(c >> 8));
                        WriteHex((byte)(c & 0xFF));
                        currentSegmentStart = i + 1;
                    }
                    break;
            }

            if (!escapeSequence.IsEmpty)
            {
                // Write any pending non-escaped segment
                if (i > currentSegmentStart)
                {
                    WriteString(value.Slice(currentSegmentStart, i - currentSegmentStart));
                }
                WriteBytes(escapeSequence);
                currentSegmentStart = i + 1;
            }
        }

        // Write any remaining non-escaped segment
        if (value.Length > currentSegmentStart)
        {
            WriteString(value.Slice(currentSegmentStart, value.Length - currentSegmentStart));
        }
    }

    /// <summary>
    /// Writes a string value.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteValue(string? value)
    {
        if (value is null)
        {
            WriteNull();
            return;
        }

        WriteByte((byte)'"');
        WriteStringEscaped(value);
        WriteByte((byte)'"');
    }

    /// <summary>
    /// Writes an integer value.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    public void WriteValue(int value)
    {
        WriteIntegerValue(value);
    }

    /// <summary>
    /// Writes a long integer value.
    /// </summary>
    /// <param name="value">The long integer value to write.</param>
    public void WriteValue(long value)
    {
        WriteLongValue(value);
    }

    /// <summary>
    /// Writes a float value.
    /// </summary>
    /// <param name="value">The float value to write.</param>
    public void WriteValue(float value)
    {
        if (!System.Buffers.Text.Utf8Formatter.TryFormat(value, _buffer[_position..], out int bytesWritten, 'G'))
            throw new InvalidOperationException($"Buffer overflow while writing float at position {_position}.");
        _position += bytesWritten;
    }

    /// <summary>
    /// Writes a double value.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteValue(double value)
    {
        if (!System.Buffers.Text.Utf8Formatter.TryFormat(value, _buffer[_position..], out int bytesWritten, 'G'))
            throw new InvalidOperationException($"Buffer overflow while writing double at position {_position}.");
        _position += bytesWritten;
    }

    /// <summary>
    /// Writes a decimal value.
    /// </summary>
    /// <param name="value">The decimal value to write.</param>
    public void WriteValue(decimal value)
    {
        if (!System.Buffers.Text.Utf8Formatter.TryFormat(value, _buffer[_position..], out int bytesWritten, 'G'))
            throw new InvalidOperationException($"Buffer overflow while writing decimal at position {_position}.");
        _position += bytesWritten;
    }

    /// <summary>
    /// Writes a boolean value.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    public void WriteValue(bool value)
    {
        var bytes = value ? "true"u8 : "false"u8;
        WriteBytes(bytes);
    }

    /// <summary>
    /// Writes a JSON null value.
    /// </summary>
    public void WriteNull()
    {
        WriteBytes("null"u8);
    }

    /// <summary>
    /// Writes a comma separator.
    /// </summary>
    public void WriteSeparator()
    {
        WriteByte((byte)',');
    }

    /// <summary>
    /// Writes a single byte to the buffer.
    /// </summary>
    /// <param name="value">The byte to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteByte(byte value)
    {
        if (_position >= _buffer.Length)
            throw new InvalidOperationException($"Buffer overflow at position {_position}");

        _buffer[_position++] = value;
    }

    /// <summary>
    /// Writes a span of bytes to the buffer.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    /// <summary>
    /// Writes a span of bytes to the buffer.
    /// </summary>
    /// <param name="bytes">The bytes to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteBytes(ReadOnlySpan<byte> bytes)
    {
        if (_position + bytes.Length > _buffer.Length)
            throw new InvalidOperationException($"Buffer overflow at position {_position}");

        if (Sse2.IsSupported && bytes.Length >= Vector128<byte>.Count)
        {
            int i = 0;
            for (; i <= bytes.Length - Vector128<byte>.Count; i += Vector128<byte>.Count)
            {
                Vector128.LoadUnsafe(ref Unsafe.AsRef(in bytes[i])).StoreUnsafe(ref _buffer[_position + i]);
            }
            // Copy remaining bytes
            if (i < bytes.Length)
            {
                bytes.Slice(i).CopyTo(_buffer.Slice(_position + i));
            }
        }
        else
        {
            bytes.CopyTo(_buffer[_position..]);
        }
        _position += bytes.Length;
    }

    /// <summary>
    /// Writes a new line and indents the output based on the current indentation level.
    /// </summary>
    private void WriteNewLineAndIndent()
    {
        WriteByte((byte)'\n');
        for (int i = 0; i < _indentLevel * 2; i++)
        {
            WriteByte((byte)' ');
        }
    }

    /// <summary>
    /// Writes an integer value to the buffer.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    private void WriteIntegerValue(int value)
    {
        if (value == 0)
        {
            WriteByte((byte)'0');
            return;
        }

        Span<byte> buffer = stackalloc byte[11]; // Max int32 length
        var isNegative = value < 0;
        var absValue = isNegative ? (uint)(-value) : (uint)value;

        var pos = buffer.Length;
        do
        {
            buffer[--pos] = (byte)('0' + (absValue % 10));
            absValue /= 10;
        } while (absValue > 0);

        if (isNegative)
            buffer[--pos] = (byte)'-';

        // Directly copy to the internal buffer to avoid Span<T> exposure issues
        var spanToWrite = buffer[pos..];
        if (_position + spanToWrite.Length > _buffer.Length)
            throw new InvalidOperationException($"Buffer overflow at position {_position}");

        spanToWrite.CopyTo(_buffer[_position..]);
        _position += spanToWrite.Length;
    }

    /// <summary>
    /// Writes a long integer value to the buffer.
    /// </summary>
    /// <param name="value">The long integer value to write.</param>
    private void WriteLongValue(long value)
    {
        if (value == 0)
        {
            WriteByte((byte)'0');
            return;
        }

        Span<byte> buffer = stackalloc byte[20]; // Max int64 length
        var isNegative = value < 0;
        var absValue = isNegative ? (ulong)(-value) : (ulong)value;

        var pos = buffer.Length;
        do
        {
            buffer[--pos] = (byte)('0' + (absValue % 10));
            absValue /= 10;
        } while (absValue > 0);

        if (isNegative)
            buffer[--pos] = (byte)'-';

        // Directly copy to the internal buffer to avoid Span<T> exposure issues
        var spanToWrite = buffer[pos..];
        if (_position + spanToWrite.Length > _buffer.Length)
            throw new InvalidOperationException($"Buffer overflow at position {_position}");

        spanToWrite.CopyTo(_buffer[_position..]);
        _position += spanToWrite.Length;
    }

    /// <summary>
    /// Writes a hexadecimal byte value.
    /// </summary>
    /// <param name="value">The byte value to write as hex.</param>
    private void WriteHex(byte value)
    {
        const string hexChars = "0123456789ABCDEF";
        WriteByte((byte)hexChars[value >> 4]);
        WriteByte((byte)hexChars[value & 0x0F]);
    }

    /// <summary>
    /// Determines if a string value needs escaping for JSON.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <returns>True if the string needs escaping, otherwise false.</returns>
    private static bool NeedsEscaping(ReadOnlySpan<char> value)
    {
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (c == '"' || c == '\\' || c < 0x20)
                return true;
        }
        return false;
    }
}
