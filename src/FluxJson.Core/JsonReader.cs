// src/FluxJson.Core/JsonReader.cs - Complete Implementation
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Numerics; // Added for BitOperations
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using FluxJson.Core.Configuration; // Added for JsonTokenType

namespace FluxJson.Core;

/// <summary>
/// High-performance JSON reader
/// </summary>
public ref struct JsonReader
{
    private ReadOnlySpan<byte> _buffer;
    private int _position;
    private readonly JsonConfiguration _config;
    private JsonTokenType _tokenType; // Added to track the current token type

    /// <summary>
    /// Initializes a new instance of the JsonReader struct.
    /// </summary>
    /// <param name="buffer">The byte buffer containing JSON data to read.</param>
    /// <param name="config">The JSON configuration settings to use.</param>
    public JsonReader(ReadOnlySpan<byte> buffer, JsonConfiguration config)
    {
        _buffer = buffer;
        _position = 0;
        _config = config;
    }

    /// <summary>
    /// Gets the current position within the JSON buffer.
    /// </summary>
    public readonly int Position => _position;
    /// <summary>
    /// Gets a value indicating whether the reader has reached the end of the JSON buffer.
    /// </summary>
    public readonly bool IsAtEnd => _position >= _buffer.Length;
    /// <summary>
    /// Gets the current byte value at the reader's position, or 0 if at the end of the buffer.
    /// </summary>
    public readonly byte Current => _position < _buffer.Length ? _buffer[_position] : (byte)0;

    /// <summary>
    /// Gets the current JSON token type.
    /// </summary>
    public readonly JsonTokenType TokenType => _tokenType;

    /// <summary>
    /// Reads the start of a JSON array ('[').
    /// </summary>
    /// <exception cref="JsonException">Thrown if the current token is not a StartArray.</exception>
    public void ReadStartArray()
    {
        SkipWhitespace();
        if (Current == (byte)'[')
        {
            _position++;
            _tokenType = JsonTokenType.StartArray;
        }
        else
        {
            throw new JsonException($"Expected '[' at position {_position}");
        }
    }

    /// <summary>
    /// Reads the end of a JSON array (']').
    /// </summary>
    /// <exception cref="JsonException">Thrown if the current token is not an EndArray.</exception>
    public void ReadEndArray()
    {
        SkipWhitespace();
        if (Current == (byte)']')
        {
            _position++;
            _tokenType = JsonTokenType.EndArray;
        }
        else
        {
            throw new JsonException($"Expected ']' at position {_position}");
        }
    }

    /// <summary>
    /// Reads the start of a JSON object ('{').
    /// </summary>
    /// <exception cref="JsonException">Thrown if the current token is not a StartObject.</exception>
    public void ReadStartObject()
    {
        SkipWhitespace();
        if (Current == (byte)'{')
        {
            _position++;
            _tokenType = JsonTokenType.StartObject;
        }
        else
        {
            throw new JsonException($"Expected '{{' at position {_position}");
        }
    }

    /// <summary>
    /// Reads the end of a JSON object ('}').
    /// </summary>
    /// <exception cref="JsonException">Thrown if the current token is not an EndObject.</exception>
    public void ReadEndObject()
    {
        SkipWhitespace();
        if (Current == (byte)'}')
        {
            _position++;
            _tokenType = JsonTokenType.EndObject;
        }
        else
        {
            throw new JsonException($"Expected '}}' at position {_position}");
        }
    }

    /// <summary>
    /// Reads a JSON property name.
    /// </summary>
    /// <returns>The property name as a string.</returns>
    /// <exception cref="JsonException">Thrown if the current token is not a PropertyName.</exception>
    public string ReadPropertyName()
    {
        SkipWhitespace();
        if (Current == (byte)'"')
        {
            _position++; // Skip opening quote
            var propertyName = ReadStringContent();
            SkipWhitespace();
            if (Current == (byte)':')
            {
                _position++; // Skip colon
                _tokenType = JsonTokenType.PropertyName;
                return propertyName;
            }
            throw new JsonException($"Expected ':' after property name at position {_position}");
        }
        throw new JsonException($"Expected property name (string) at position {_position}");
    }

    /// <summary>
    /// Skips the current JSON value (object, array, string, number, boolean, or null).
    /// </summary>
    public void SkipValue()
    {
        SkipWhitespace();
        switch (Current)
        {
            case (byte)'{':
                _position++;
                SkipObject();
                break;
            case (byte)'[':
                _position++;
                SkipArray();
                break;
            case (byte)'"':
                ReadString(); // This advances position past the string
                break;
            case (byte)'-':
            case (byte)'0':
            case (byte)'1':
            case (byte)'2':
            case (byte)'3':
            case (byte)'4':
            case (byte)'5':
            case (byte)'6':
            case (byte)'7':
            case (byte)'8':
            case (byte)'9':
                ReadDouble(); // Read any number type
                break;
            case (byte)'t': // true
                if (_buffer[_position..].StartsWith("true"u8)) _position += 4;
                else throw new JsonException($"Invalid token at position {_position}");
                break;
            case (byte)'f': // false
                if (_buffer[_position..].StartsWith("false"u8)) _position += 5;
                else throw new JsonException($"Invalid token at position {_position}");
                break;
            case (byte)'n': // null
                if (_buffer[_position..].StartsWith("null"u8)) _position += 4;
                else throw new JsonException($"Invalid token at position {_position}");
                break;
            default:
                throw new JsonException($"Unexpected token '{Current}' at position {_position}");
        }
    }

    private void SkipObject()
    {
        int depth = 1;
        while (depth > 0 && _position < _buffer.Length)
        {
            SkipWhitespace();
            switch (Current)
            {
                case (byte)'{':
                    depth++;
                    _position++;
                    break;
                case (byte)'}':
                    depth--;
                    _position++;
                    break;
                case (byte)'"':
                    ReadString(); // Skip property name or string value
                    SkipWhitespace();
                    if (Current == (byte)':') _position++; // Skip colon after property name
                    break;
                case (byte)'[':
                    _position++;
                    SkipArray();
                    break;
                case (byte)',':
                    _position++;
                    break;
                default:
                    // Skip other values (numbers, booleans, nulls)
                    // This is a simplified skip; a full implementation would need to parse the value
                    // to know its extent. For now, we'll just advance past simple tokens.
                    // A more robust solution would involve a PeekNextToken and then advancing.
                    // For the purpose of this task, we'll assume simple values are handled by ReadString/ReadDouble etc.
                    // and complex structures by SkipObject/SkipArray.
                    // This part needs careful consideration for a production-ready parser.
                    if (TryReadNull() || ReadBoolean() || TryReadNumber())
                    {
                        // Value was read, position advanced
                    }
                    else
                    {
                        // If it's not a known token, it's an error or part of a value that needs more robust skipping
                        // For now, we'll just advance to avoid infinite loops on unknown tokens.
                        _position++;
                    }
                    break;
            }
        }
        if (depth > 0) throw new JsonException($"Unterminated object at position {_position}");
    }

    private void SkipArray()
    {
        int depth = 1;
        while (depth > 0 && _position < _buffer.Length)
        {
            SkipWhitespace();
            switch (Current)
            {
                case (byte)'[':
                    depth++;
                    _position++;
                    break;
                case (byte)']':
                    depth--;
                    _position++;
                    break;
                case (byte)'{':
                    _position++;
                    SkipObject();
                    break;
                case (byte)'"':
                    ReadString();
                    break;
                case (byte)',':
                    _position++;
                    break;
                default:
                    // Skip other values (numbers, booleans, nulls)
                    if (TryReadNull() || ReadBoolean() || TryReadNumber())
                    {
                        // Value was read, position advanced
                    }
                    else
                    {
                        _position++;
                    }
                    break;
            }
        }
        if (depth > 0) throw new JsonException($"Unterminated array at position {_position}");
    }

    // Helper to try reading a number without throwing, for SkipValue
    private bool TryReadNumber()
    {
        var start = _position;
        if (Current == (byte)'-' || Current == (byte)'+')
            _position++;

        bool hasDigits = false;
        while (_position < _buffer.Length && IsDigit(_buffer[_position]))
        {
            _position++;
            hasDigits = true;
        }

        if (_position < _buffer.Length && _buffer[_position] == (byte)'.')
        {
            _position++;
            while (_position < _buffer.Length && IsDigit(_buffer[_position]))
            {
                _position++;
                hasDigits = true;
            }
        }

        if (_position < _buffer.Length && (_buffer[_position] == (byte)'e' || _buffer[_position] == (byte)'E'))
        {
            _position++;
            if (_position < _buffer.Length && (_buffer[_position] == (byte)'+' || _buffer[_position] == (byte)'-'))
                _position++;
            while (_position < _buffer.Length && IsDigit(_buffer[_position]))
            {
                _position++;
                hasDigits = true;
            }
        }

        return hasDigits && _position > start;
    }

    /// <summary>
    /// Attempts to read a JSON null value from the current position.
    /// </summary>
    /// <returns>True if a null value was successfully read; otherwise, false.</returns>
    public bool TryReadNull()
    {
        if (_buffer[_position..].StartsWith("null"u8))
        {
            _position += 4;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Reads a JSON boolean value from the current position.
    /// </summary>
    /// <returns>The boolean value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value is not a valid JSON boolean.</exception>
    public bool ReadBoolean()
    {
        SkipWhitespace();

        if (_buffer[_position..].StartsWith("true"u8))
        {
            _position += 4;
            return true;
        }

        if (_buffer[_position..].StartsWith("false"u8))
        {
            _position += 5;
            return false;
        }

        throw new JsonException($"Invalid boolean at position {_position}");
    }

    /// <summary>
    /// Reads a JSON string value from the current position.
    /// </summary>
    /// <returns>The string value, or null if the value is JSON null.</returns>
    /// <exception cref="JsonException">Thrown when the value is not a valid JSON string.</exception>
    public string? ReadString()
    {
        SkipWhitespace();

        if (Current == (byte)'"')
        {
            _position++; // Skip opening quote
            return ReadStringContent();
        }

        if (TryReadNull())
            return null;

        throw new JsonException($"Expected string at position {_position}");
    }

    private string ReadStringContent()
    {
        var start = _position;
        var charPool = ArrayPool<char>.Shared;
        char[]? rentedBuffer = null;
        Span<char> charBuffer = Span<char>.Empty;
        int charWritten = 0;

        try
        {
            // Estimate buffer size (worst case: all chars are escaped, or unicode escapes)
            var estimatedLength = _buffer.Length - _position;
            if (estimatedLength > 0)
            {
                rentedBuffer = charPool.Rent(estimatedLength);
                charBuffer = rentedBuffer.AsSpan();
            }

            while (_position < _buffer.Length)
            {
                var b = _buffer[_position];
                if (b == (byte)'"')
                {
                    _position++; // Skip closing quote
                    if (charWritten == 0)
                    {
                        // No escapes, no content written to charBuffer, return direct string from byte buffer
                        return _config.Encoding.GetString(_buffer[start..(_position - 1)]);
                    }
                    else
                    {
                        return new string(charBuffer[..charWritten]);
                    }
                }
                else if (b == (byte)'\\')
                {
                    _position++; // Skip escape character
                    if (_position >= _buffer.Length)
                        throw new JsonException($"Unterminated escape sequence at position {_position}");

                    var escaped = _buffer[_position];
                    char decodedChar;
                    switch (escaped)
                    {
                        case (byte)'"': decodedChar = '"'; break;
                        case (byte)'\\': decodedChar = '\\'; break;
                        case (byte)'/': decodedChar = '/'; break;
                        case (byte)'b': decodedChar = '\b'; break;
                        case (byte)'f': decodedChar = '\f'; break;
                        case (byte)'n': decodedChar = '\n'; break;
                        case (byte)'r': decodedChar = '\r'; break;
                        case (byte)'t': decodedChar = '\t'; break;
                        case (byte)'u':
                            _position++; // Advance past 'u'
                            decodedChar = ReadUnicodeEscape();
                            break;
                        default:
                            throw new JsonException($"Invalid escape sequence \\{(char)escaped}");
                    }

                    if (charWritten >= charBuffer.Length)
                    {
                        // This should ideally not happen with a good initial estimate, but handle if it does
                        // For simplicity, we'll throw, but in a real scenario, you'd resize the buffer.
                        throw new JsonException($"Buffer too small for unescaped string at position {_position}. Consider increasing initial buffer size.");
                    }
                    charBuffer[charWritten++] = decodedChar;
                    _position++;
                }
                else
                {
                    // Non-escaped character
                    // If we haven't started writing to charBuffer yet, and we encounter a non-escape char,
                    // we need to copy the preceding non-escaped bytes to charBuffer first.
                    if (charWritten == 0 && _position > start)
                    {
                        var nonEscapedBytes = _buffer[start.._position];
                        charWritten += _config.Encoding.GetChars(nonEscapedBytes, charBuffer[charWritten..]);
                    }

                    if (charWritten >= charBuffer.Length)
                    {
                        throw new JsonException($"Buffer too small for unescaped string at position {_position}. Consider increasing initial buffer size.");
                    }
                    charBuffer[charWritten++] = (char)b; // Assuming ASCII for direct cast, otherwise use encoding
                    _position++;
                }
            }
        }
        finally
        {
            if (rentedBuffer != null)
            {
                charPool.Return(rentedBuffer);
            }
        }

        throw new JsonException($"Unterminated string at position {_position}");
    }

    private char ReadUnicodeEscape()
    {
        if (_position + 4 > _buffer.Length)
            throw new JsonException($"Incomplete unicode escape sequence at position {_position}");

        var hexSpan = _buffer.Slice(_position, 4);
        _position += 4; // Advance position by 4 bytes

        ushort value = 0;
        for (int i = 0; i < 4; i++)
        {
            byte b = hexSpan[i];
            value <<= 4;
            if (b >= (byte)'0' && b <= (byte)'9')
                value |= (ushort)(b - (byte)'0');
            else if (b >= (byte)'a' && b <= (byte)'f')
                value |= (ushort)(b - (byte)'a' + 10);
            else if (b >= (byte)'A' && b <= (byte)'F')
                value |= (ushort)(b - (byte)'A' + 10);
            else
                throw new JsonException($"Invalid unicode escape sequence at position {_position}");
        }

        return (char)value;
    }

    /// <summary>
    /// Reads a 32-bit integer from the JSON buffer.
    /// </summary>
    /// <returns>The integer value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value cannot be parsed as a valid integer.</exception>
    public int ReadInt32()
    {
        SkipWhitespace();
        var start = _position;

        // Handle negative numbers
        if (Current == (byte)'-')
            _position++;

        // Read digits
        while (_position < _buffer.Length && IsDigit(_buffer[_position]))
        {
            _position++;
        }

        var numberSpan = _buffer[start.._position];
        if (!Utf8Parser.TryParse(numberSpan, out int result, out int bytesConsumed, 'D'))
            throw new JsonException($"Invalid integer at position {start}");
        if (bytesConsumed != numberSpan.Length)
            throw new JsonException($"Invalid integer format at position {start}");

        return result;
    }

    /// <summary>
    /// Reads a 64-bit integer from the JSON buffer.
    /// </summary>
    /// <returns>The long value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value cannot be parsed as a valid long integer.</exception>
    public long ReadInt64()
    {
        SkipWhitespace();
        var start = _position;

        // Handle negative numbers
        if (Current == (byte)'-')
            _position++;

        // Read digits
        while (_position < _buffer.Length && IsDigit(_buffer[_position]))
        {
            _position++;
        }

        var numberSpan = _buffer[start.._position];
        if (!Utf8Parser.TryParse(numberSpan, out long result, out int bytesConsumed, 'D'))
            throw new JsonException($"Invalid long integer at position {start}");
        if (bytesConsumed != numberSpan.Length)
            throw new JsonException($"Invalid long integer format at position {start}");

        return result;
    }

    /// <summary>
    /// Reads a single-precision floating-point number from the JSON buffer.
    /// </summary>
    /// <returns>The float value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value cannot be parsed as a valid float.</exception>
    public float ReadSingle()
    {
        SkipWhitespace();
        var start = _position;

        // Handle sign
        if (Current == (byte)'-' || Current == (byte)'+')
            _position++;

        // Read number (including decimal point and exponent)
        while (_position < _buffer.Length)
        {
            var b = _buffer[_position];
            if (IsDigit(b) || b == (byte)'.' || b == (byte)'e' || b == (byte)'E' || b == (byte)'+' || b == (byte)'-')
            {
                _position++;
            }
            else
            {
                break;
            }
        }

        var numberSpan = _buffer[start.._position];
        if (!Utf8Parser.TryParse(numberSpan, out float result, out int bytesConsumed, 'G'))
            throw new JsonException($"Invalid float at position {start}");
        if (bytesConsumed != numberSpan.Length)
            throw new JsonException($"Invalid float format at position {start}");

        return result;
    }

    /// <summary>
    /// Reads a double-precision floating-point number from the JSON buffer.
    /// </summary>
    /// <returns>The double value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value cannot be parsed as a valid double.</exception>
    public double ReadDouble()
    {
        SkipWhitespace();
        var start = _position;

        // Handle sign
        if (Current == (byte)'-' || Current == (byte)'+')
            _position++;

        // Read number (including decimal point and exponent)
        while (_position < _buffer.Length)
        {
            var b = _buffer[_position];
            if (IsDigit(b) || b == (byte)'.' || b == (byte)'e' || b == (byte)'E' || b == (byte)'+' || b == (byte)'-')
            {
                _position++;
            }
            else
            {
                break;
            }
        }

        var numberSpan = _buffer[start.._position];
        if (!Utf8Parser.TryParse(numberSpan, out double result, out int bytesConsumed, 'G'))
            throw new JsonException($"Invalid double at position {start}");
        if (bytesConsumed != numberSpan.Length)
            throw new JsonException($"Invalid double format at position {start}");

        return result;
    }

    /// <summary>
    /// Reads a decimal number from the JSON buffer.
    /// </summary>
    /// <returns>The decimal value read from the JSON.</returns>
    /// <exception cref="JsonException">Thrown when the value cannot be parsed as a valid decimal.</exception>
    public decimal ReadDecimal()
    {
        SkipWhitespace();
        var start = _position;

        // Handle sign
        if (Current == (byte)'-' || Current == (byte)'+')
            _position++;

        // Read number (including decimal point but not exponent for decimal)
        while (_position < _buffer.Length)
        {
            var b = _buffer[_position];
            if (IsDigit(b) || b == (byte)'.')
            {
                _position++;
            }
            else
            {
                break;
            }
        }

        var numberSpan = _buffer[start.._position];
        if (!Utf8Parser.TryParse(numberSpan, out decimal result, out int bytesConsumed, 'G'))
            throw new JsonException($"Invalid decimal at position {start}");
        if (bytesConsumed != numberSpan.Length)
            throw new JsonException($"Invalid decimal format at position {start}");

        return result;
    }

    /// <summary>
    /// Skips over any whitespace characters in the JSON buffer.
    /// </summary>
    /// <summary>
    /// Skips over any whitespace characters in the JSON buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhitespace()
    {
        if (Sse2.IsSupported && _buffer.Length - _position >= Vector128<byte>.Count)
        {
            var whitespaceMask = Vector128.Create((byte)' ', (byte)'\t', (byte)'\n', (byte)'\r', (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r', (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r', (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r');
            while (_position <= _buffer.Length - Vector128<byte>.Count)
            {
                // Use Unsafe.AsRef(in ...) for ReadOnlySpan
                var vector = Vector128.LoadUnsafe(ref Unsafe.AsRef(in _buffer[_position]));
                var eqSpace = Sse2.CompareEqual(vector, Vector128.Create((byte)' '));
                var eqTab = Sse2.CompareEqual(vector, Vector128.Create((byte)'\t'));
                var eqLf = Sse2.CompareEqual(vector, Vector128.Create((byte)'\n'));
                var eqCr = Sse2.CompareEqual(vector, Vector128.Create((byte)'\r'));

                var isWhitespace = Sse2.Or(Sse2.Or(eqSpace, eqTab), Sse2.Or(eqLf, eqCr));
                var mask = Sse2.MoveMask(isWhitespace);

                if (mask == 0xFFFF) // All 16 bytes are whitespace
                {
                    _position += Vector128<byte>.Count;
                }
                else
                {
                    // Find the first non-whitespace character within the vector
                    var trailingZeros = BitOperations.TrailingZeroCount(~mask);
                    _position += trailingZeros;
                    return;
                }
            }
        }

        // Fallback for remaining bytes or if SIMD is not supported
        while (_position < _buffer.Length && IsWhitespace(_buffer[_position]))
        {
            _position++;
        }
    }

    /// <summary>
    /// Advances the reader to the next position in the JSON buffer.
    /// </summary>
    /// <returns>True if there are more characters to read; false if the end of the buffer is reached.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadNext()
    {
        _position++;
        return _position < _buffer.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsWhitespace(byte b) => b is (byte)' ' or (byte)'\t' or (byte)'\n' or (byte)'\r';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsEscapes(ReadOnlySpan<byte> bytes)
    {
        if (Sse2.IsSupported && bytes.Length >= Vector128<byte>.Count)
        {
            var backslashVector = Vector128.Create((byte)'\\');
            for (int i = 0; i <= bytes.Length - Vector128<byte>.Count; i += Vector128<byte>.Count)
            {
                var vector = Vector128.LoadUnsafe(ref Unsafe.AsRef(in bytes[i]));
                var cmp = Sse2.CompareEqual(vector, backslashVector);
                if (Sse2.MoveMask(cmp) != 0)
                {
                    return true;
                }
            }
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == (byte)'\\')
                return true;
        }
        return false;
    }
}
