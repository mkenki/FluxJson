// src/FluxJson.Core/FluxJsonSerializer.cs - Complete Implementation
using System.Buffers;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using FluxJson.Core.Configuration;
using FluxJson.Core.Converters;
using static FluxJson.Core.TypeHelpers;

namespace FluxJson.Core;

/// <summary>
/// High-performance JSON serializer implementation
/// </summary>
public sealed class FluxJsonSerializer : IJsonSerializer
{
    /// <summary>
    /// The JSON configuration used by this serializer.
    /// </summary>
    private readonly JsonConfiguration _config;
    /// <summary>
    /// Cache for JSON converters to improve performance.
    /// </summary>
    private readonly Dictionary<Type, IJsonConverter?> _converterCache;
    /// <summary>
    /// Array pool for renting and returning byte buffers.
    /// </summary>
    private readonly ArrayPool<byte> _bufferPool;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluxJsonSerializer"/> class.
    /// </summary>
    /// <param name="configuration">Optional JSON configuration. If null, a default configuration is used.</param>
    public FluxJsonSerializer(JsonConfiguration? configuration = null)
    {
        _config = configuration ?? new JsonConfiguration();
        _converterCache = new Dictionary<Type, IJsonConverter?>();
        _bufferPool = ArrayPool<byte>.Shared;

        // Register built-in converters
        RegisterBuiltInConverters();
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public string Serialize<T>(T value)
    {
        if (value is null)
            return "null";

        // Rent buffer from pool for better performance
        var buffer = _bufferPool.Rent(4096);
        try
        {
            var bytesWritten = SerializeToSpan(value, buffer);
            return _config.Encoding.GetString(buffer, 0, bytesWritten);
        }
        finally
        {
            _bufferPool.Return(buffer);
        }
    }

    /// <summary>
    /// Serializes an object to a byte array containing its JSON representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A byte array containing the JSON representation of the object.</returns>
    public byte[] SerializeToBytes<T>(T value)
    {
        if (value is null)
            return "null"u8.ToArray();

        var buffer = _bufferPool.Rent(4096);
        try
        {
            var bytesWritten = SerializeToSpan(value, buffer);
            var result = new byte[bytesWritten];
            Array.Copy(buffer, result, bytesWritten);
            return result;
        }
        finally
        {
            _bufferPool.Return(buffer);
        }
    }

    /// <summary>
    /// Serializes an object directly into a byte span.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="destination">The span to write the JSON bytes into.</param>
    /// <returns>The number of bytes written to the destination span.</returns>
    public int SerializeToSpan<T>(T value, Span<byte> destination)
    {
        if (value is null)
        {
            "null"u8.CopyTo(destination);
            return 4;
        }

        var writer = new JsonWriter(destination, _config);
        WriteValue(ref writer, value, typeof(T));
        return writer.Position;
    }

    /// <summary>
    /// Deserializes a JSON string into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or default value if the JSON is null or empty.</returns>
    public T? Deserialize<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        var bytes = _config.Encoding.GetBytes(json);
        return Deserialize<T>(bytes.AsSpan());
    }

    /// <summary>
    /// Deserializes a JSON byte span into an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON byte span to deserialize.</param>
    /// <returns>The deserialized object, or default value if the JSON is empty.</returns>
    public T? Deserialize<T>(ReadOnlySpan<byte> json)
    {
        if (json.IsEmpty)
            return default;

        var reader = new JsonReader(json, _config);
        return ReadValue<T>(ref reader);
    }

    /// <summary>
    /// Deserializes a JSON string into an object of the specified type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="type">The type to deserialize the JSON into.</param>
    /// <returns>The deserialized object, or null if the JSON is null or empty.</returns>
    public object? Deserialize(string json, Type type)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        var bytes = _config.Encoding.GetBytes(json);
        var reader = new JsonReader(bytes.AsSpan(), _config);
        return ReadValue(ref reader, type);
    }

    /// <summary>
    /// Asynchronously serializes an object to a stream.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="stream">The stream to write the JSON to.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    public async Task SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken = default)
    {
        var json = Serialize(value);
        var bytes = _config.Encoding.GetBytes(json);
        await stream.WriteAsync(bytes, cancellationToken);
    }

    /// <summary>
    /// Asynchronously deserializes an object from a stream.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="stream">The stream to read the JSON from.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, returning the deserialized object.</returns>
    public async Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, _config.Encoding);
        var json = await reader.ReadToEndAsync();
        return Deserialize<T>(json);
    }

    /// <summary>
    /// Writes a value to the JSON writer.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="type">The type of the value.</param>
    /// <param name="property">Optional: The property being serialized.</param>
    private void WriteValue<T>(ref JsonWriter writer, T value, Type type, PropertyInfo? property = null)
    {
        if (value is null)
        {
            if (_config.NullHandling != NullHandling.Ignore)
                writer.WriteNull();
            return;
        }

        // Check for custom converters first
        if (TryGetConverter(type, out var converter, property))
        {
            if (converter is IJsonConverter<T> typedConverter && typedConverter.CanWrite)
            {
                typedConverter.Write(ref writer, value, _config);
                return;
            }
        }

        // Handle primitive types
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
                writer.WriteValue((string)(object)value);
                break;
            case TypeCode.Boolean:
                writer.WriteValue((bool)(object)value);
                break;
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
                writer.WriteValue(Convert.ToInt32(value));
                break;
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                writer.WriteValue(Convert.ToInt64(value));
                break;
            case TypeCode.Single:
                writer.WriteValue(Convert.ToSingle(value));
                break;
            case TypeCode.Double:
                writer.WriteValue(Convert.ToDouble(value));
                break;
            case TypeCode.Decimal:
                writer.WriteValue(Convert.ToDecimal(value));
                break;
            case TypeCode.DateTime:
                WriteDateTime(ref writer, (DateTime)(object)value);
                break;
            default:
                WriteComplexValue(ref writer, value, type);
                break;
        }
    }

    /// <summary>
    /// Writes a complex value (non-primitive, non-string) to the JSON writer.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="type">The type of the value.</param>
    private void WriteComplexValue<T>(ref JsonWriter writer, T value, Type type)
    {
        // Handle nullable types
        if (IsNullableType(type))
        {
            if (value is not null)
            {
                var underlyingType = GetNullableUnderlyingType(type);
                WriteValue(ref writer, value, underlyingType, null); // No direct property context for underlying type
            }
            else
            {
                writer.WriteNull();
            }
            return;
        }

        // Handle arrays and collections
        if (IsCollectionType(type) && value is IEnumerable enumerable)
        {
            WriteArray(ref writer, enumerable, type);
            return;
        }

        // Handle dictionaries
        if (value is IDictionary dictionary)
        {
            WriteDictionary(ref writer, dictionary);
            return;
        }

        // Handle Guid
        if (type == typeof(Guid))
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((Guid)(object)value).ToString());
            }
            return;
        }

        // Handle DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            if (value is not null)
                WriteDateTimeOffset(ref writer, (DateTimeOffset)(object)value);
            else
                writer.WriteNull();
            return;
        }

        // Handle enums
        if (type.IsEnum)
        {
            WriteEnum(ref writer, value, type);
            return;
        }

        // Handle objects
        WriteObject(ref writer, value, type);
    }

    /// <summary>
    /// Writes an enumerable of a specific type to the JSON writer as an array.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="enumerable">The enumerable to write.</param>
    /// <param name="type">The type of the enumerable.</param>
    private void WriteArray(ref JsonWriter writer, IEnumerable enumerable, Type type)
    {
        writer.WriteStartArray();

        var isFirst = true;
        var elementType = GetCollectionElementType(type);

        foreach (var item in enumerable)
        {
            if (!isFirst)
                writer.WriteSeparator();

            WriteValue(ref writer, item, elementType, property: null); // No direct property context for array items
            isFirst = false;
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Writes a dictionary to the JSON writer as an object.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="dictionary">The dictionary to write.</param>
    private void WriteDictionary(ref JsonWriter writer, IDictionary dictionary)
    {
        writer.WriteStartObject();

        var isFirst = true;
        foreach (DictionaryEntry entry in dictionary)
        {
            if (!isFirst)
                writer.WriteSeparator();

            var keyString = entry.Key?.ToString() ?? "null";
            writer.WritePropertyName(keyString);
            WriteValue(ref writer, entry.Value, entry.Value?.GetType() ?? typeof(object), property: null); // No direct property context for dictionary values
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes an object's properties to the JSON writer.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The object to write.</param>
    /// <param name="type">The type of the object.</param>
    private void WriteObject<T>(ref JsonWriter writer, T value, Type type)
    {
        writer.WriteStartObject();

        var properties = GetSerializableProperties(type);
        var isFirst = true;

        foreach (var property in properties)
        {
            var propValue = property.GetValue(value);

            // Handle null values based on configuration
            if (propValue is null && _config.NullHandling == NullHandling.Ignore)
                continue;

            if (!isFirst)
                writer.WriteSeparator();

            var propertyName = GetPropertyName(property.Name);
            writer.WritePropertyName(propertyName);

            WriteValue(ref writer, propValue, property.PropertyType, property);
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a DateTime value to the JSON writer based on the configured format.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="dateTime">The DateTime value to write.</param>
    private void WriteDateTime(ref JsonWriter writer, DateTime dateTime)
    {
        var formatted = _config.DateTimeFormat switch
        {
            DateTimeFormat.ISO8601 => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            DateTimeFormat.UnixTimestamp => ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            DateTimeFormat.Custom => dateTime.ToString(_config.CustomDateTimeFormat, CultureInfo.InvariantCulture),
            _ => dateTime.ToString("O", CultureInfo.InvariantCulture) // Default ISO 8601
        };

        writer.WriteValue(formatted);
    }

    /// <summary>
    /// Writes a DateTimeOffset value to the JSON writer based on the configured format.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="dateTimeOffset">The DateTimeOffset value to write.</param>
    private void WriteDateTimeOffset(ref JsonWriter writer, DateTimeOffset dateTimeOffset)
    {
        var formatted = _config.DateTimeFormat switch
        {
            DateTimeFormat.ISO8601 => dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture),
            DateTimeFormat.UnixTimestamp => dateTimeOffset.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            _ => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture)
        };

        writer.WriteValue(formatted);
    }

    /// <summary>
    /// Writes an enum value to the JSON writer as its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the enum.</typeparam>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="value">The enum value to write.</param>
    /// <param name="type">The type of the enum.</param>
    private void WriteEnum<T>(ref JsonWriter writer, T value, Type type)
    {
        // Convert enum to string representation
        var enumString = value?.ToString() ?? "null";
        writer.WriteValue(enumString);
    }

    /// <summary>
    /// Reads a JSON value and deserializes it into an object of the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="reader">The JSON reader.</param>
    /// <returns>The deserialized object.</returns>
    private T? ReadValue<T>(ref JsonReader reader)
    {
        return (T?)ReadValue(ref reader, typeof(T));
    }

    /// <summary>
    /// Reads a JSON value and deserializes it into an object of the specified type.
    /// </summary>
    /// <summary>
    /// Reads a JSON value and deserializes it into an object of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="type">The type to deserialize the JSON into.</param>
    /// <param name="property">Optional: The property being deserialized.</param>
    /// <returns>The deserialized object.</returns>
    private object? ReadValue(ref JsonReader reader, Type type, PropertyInfo? property = null)
    {
        reader.SkipWhitespace();

        if (reader.IsAtEnd)
            return GetDefaultValue(type);

        // Check for null
        if (reader.Current == (byte)'n' && reader.TryReadNull())
            return null;

        // Check for custom converters
        if (TryGetConverter(type, out var converter, property))
        {
            // This is a placeholder for a proper fix.
            // The previous reflection-based approach failed due to JsonReader being a ref struct.
            // A more robust solution is needed here.
            if (converter is IJsonConverter<object> genericConverter && genericConverter.CanRead)
            {
                return genericConverter.Read(ref reader, _config);
            }
        }

        // Handle nullable types
        if (IsNullableType(type))
        {
            var underlyingType = GetNullableUnderlyingType(type);
            return ReadValue(ref reader, underlyingType, property); // Pass property context
        }

        // Handle primitive types
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
                return reader.ReadString();
            case TypeCode.Boolean:
                return reader.ReadBoolean();
            case TypeCode.Byte:
                return (byte)reader.ReadInt32();
            case TypeCode.SByte:
                return (sbyte)reader.ReadInt32();
            case TypeCode.Int16:
                return (short)reader.ReadInt32();
            case TypeCode.UInt16:
                return (ushort)reader.ReadInt32();
            case TypeCode.Int32:
                return reader.ReadInt32();
            case TypeCode.UInt32:
                return (uint)reader.ReadInt64();
            case TypeCode.Int64:
                return reader.ReadInt64();
            case TypeCode.UInt64:
                return (ulong)reader.ReadInt64();
            case TypeCode.Single:
                return reader.ReadSingle();
            case TypeCode.Double:
                return reader.ReadDouble();
            case TypeCode.Decimal:
                return reader.ReadDecimal();
            case TypeCode.DateTime:
                return ReadDateTime(ref reader);
            default:
                return ReadComplexValue(ref reader, type);
        }
    }

    /// <summary>
    /// Reads a complex JSON value (object or array) and deserializes it into an object of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="type">The type to deserialize the JSON into.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="JsonException">Thrown if the type is unsupported or JSON format is invalid.</exception>
    private object? ReadComplexValue(ref JsonReader reader, Type type)
    {
        reader.SkipWhitespace();

        // Handle arrays and collections
        if (reader.Current == (byte)'[')
        {
            return ReadArray(ref reader, type);
        }

        // Handle objects and dictionaries
        if (reader.Current == (byte)'{')
        {
            if (IsDictionaryType(type))
                return ReadDictionary(ref reader, type);
            else
                return ReadObject(ref reader, type);
        }

        // Handle Guid
        if (type == typeof(Guid))
        {
            var guidString = reader.ReadString();
            return guidString != null ? Guid.Parse(guidString) : Guid.Empty;
        }

        // Handle DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            return ReadDateTimeOffset(ref reader);
        }

        // Handle enums
        if (type.IsEnum)
        {
            return ReadEnum(ref reader, type);
        }

        throw new JsonException($"Unsupported type: {type.Name} at position {reader.Position}");
    }

    /// <summary>
    /// Reads a JSON array and deserializes it into a collection of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="type">The target collection type.</param>
    /// <returns>The deserialized collection.</returns>
    private object? ReadArray(ref JsonReader reader, Type type)
    {
        reader.ReadNext(); // Skip '['
        reader.SkipWhitespace();

        var elementType = GetCollectionElementType(type);
        var items = new List<object?>();

        while (!reader.IsAtEnd && reader.Current != (byte)']')
        {
            var item = ReadValue(ref reader, elementType, property: null); // No direct property context for array items
            items.Add(item);

            reader.SkipWhitespace();

            if (reader.Current == (byte)',')
            {
                reader.ReadNext(); // Skip ','
                reader.SkipWhitespace();
            }
        }

        if (reader.Current == (byte)']')
            reader.ReadNext(); // Skip ']'

        // Convert to appropriate collection type
        return CreateCollectionInstance(type, elementType, items.Count);
    }

    /// <summary>
    /// Reads a JSON object and deserializes it into a dictionary of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="type">The target dictionary type.</param>
    /// <returns>The deserialized dictionary.</returns>
    /// <exception cref="JsonException">Thrown if the JSON format is invalid.</exception>
    private object? ReadDictionary(ref JsonReader reader, Type type)
    {
        reader.ReadNext(); // Skip '{'

        var keyType = GetCollectionElementType(typeof(IDictionary<,>).MakeGenericType(type.GetGenericArguments()));
        var valueType = type.GetGenericArguments().Length > 1 ? type.GetGenericArguments()[1] : typeof(object);
        var dictionary = CreateDictionaryInstance(type, keyType, valueType);

        while (!reader.IsAtEnd && reader.Current != (byte)'}')
        {
            reader.SkipWhitespace();

            if (reader.Current == (byte)'}')
                break;

            // Read key
            var keyString = reader.ReadString();
            if (keyString is null)
                throw new JsonException($"Expected property name at position {reader.Position}");

            var key = ConvertKey(keyString, keyType);

            reader.SkipWhitespace();

            if (reader.Current != (byte)':')
                throw new JsonException($"Expected ':' after property name at position {reader.Position}");

            reader.ReadNext(); // Skip ':'

            // Read value
            var value = ReadValue(ref reader, valueType, property: null); // No direct property context for dictionary values
            dictionary[key] = value;

            reader.SkipWhitespace();

            if (reader.Current == (byte)',')
            {
                reader.ReadNext(); // Skip ','
            }
        }

        if (reader.Current == (byte)'}')
            reader.ReadNext(); // Skip '}'

        return dictionary;
    }

    /// <summary>
    /// Reads a JSON object and deserializes it into an instance of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="type">The target object type.</param>
    /// <returns>The deserialized object.</returns>
    /// <exception cref="JsonException">Thrown if the JSON format is invalid.</exception>
    private object ReadObject(ref JsonReader reader, Type type)
    {
        reader.SkipWhitespace();

        if (reader.Current != (byte)'{')
            throw new JsonException($"Expected '{{' at position {reader.Position}");

        reader.ReadNext(); // Skip '{'
        var instance = Activator.CreateInstance(type)!;
        var properties = GetSerializableProperties(type).ToDictionary(p => GetPropertyName(p.Name), p => p);

        while (!reader.IsAtEnd && reader.Current != (byte)'}')
        {
            reader.SkipWhitespace();

            if (reader.Current == (byte)'}')
                break;

            // Read property name
            var propertyName = reader.ReadString();
            if (propertyName is null)
                throw new JsonException($"Expected property name at position {reader.Position}");

            reader.SkipWhitespace();

            if (reader.Current != (byte)':')
                throw new JsonException($"Expected ':' after property name at position {reader.Position}");

            reader.ReadNext(); // Skip ':'

            // Find matching property
            if ((properties.TryGetValue(propertyName, out var property) ||
                (!_config.CaseSensitive && TryFindPropertyIgnoreCase(properties, propertyName, out property))) &&
                property?.PropertyType != null)
            {
                var value = ReadValue(ref reader, property.PropertyType, property);
                property.SetValue(instance, value);
            }
            else
            {
                // Skip unknown property value
                SkipValue(ref reader);
            }

            reader.SkipWhitespace();

            if (reader.Current == (byte)',')
            {
                reader.ReadNext(); // Skip ','
            }
        }

        if (reader.Current == (byte)'}')
            reader.ReadNext(); // Skip '}'

        return instance;
    }

    /// <summary>
    /// Reads a JSON string and deserializes it into a DateTime object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <returns>The deserialized DateTime object.</returns>
    /// <exception cref="JsonException">Thrown if the JSON string is null or cannot be parsed.</exception>
    private DateTime ReadDateTime(ref JsonReader reader)
    {
        var dateString = reader.ReadString();
        if (dateString is null)
            throw new JsonException($"Expected date string at position {reader.Position}");

        return _config.DateTimeFormat switch
        {
            DateTimeFormat.UnixTimestamp when long.TryParse(dateString, out var timestamp)
                => DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
            DateTimeFormat.Custom when _config.CustomDateTimeFormat != null
                => DateTime.ParseExact(dateString, _config.CustomDateTimeFormat, CultureInfo.InvariantCulture),
            _ => DateTime.Parse(dateString, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Reads a JSON string and deserializes it into a DateTimeOffset object.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <returns>The deserialized DateTimeOffset object.</returns>
    /// <exception cref="JsonException">Thrown if the JSON string is null or cannot be parsed.</exception>
    private DateTimeOffset ReadDateTimeOffset(ref JsonReader reader)
    {
        var dateString = reader.ReadString();
        if (dateString is null)
            throw new JsonException($"Expected date string at position {reader.Position}");

        return _config.DateTimeFormat switch
        {
            DateTimeFormat.UnixTimestamp when long.TryParse(dateString, out var timestamp)
                => DateTimeOffset.FromUnixTimeSeconds(timestamp),
            _ => DateTimeOffset.Parse(dateString, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Reads a JSON string and deserializes it into an enum value of the specified type.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    /// <param name="enumType">The target enum type.</param>
    /// <returns>The deserialized enum value.</returns>
    /// <exception cref="JsonException">Thrown if the JSON string is null or cannot be parsed into the enum type.</exception>
    private object ReadEnum(ref JsonReader reader, Type enumType)
    {
        var enumString = reader.ReadString();
        if (enumString is null)
            throw new JsonException($"Expected enum string at position {reader.Position}");

        return Enum.Parse(enumType, enumString);
    }

    /// <summary>
    /// Converts a string key to the specified key type for dictionaries.
    /// </summary>
    /// <param name="keyString">The string representation of the key.</param>
    /// <param name="keyType">The target key type.</param>
    /// <returns>The converted key.</returns>
    private object ConvertKey(string keyString, Type keyType)
    {
        if (keyType == typeof(string))
            return keyString;

        if (keyType == typeof(int))
            return int.Parse(keyString);

        if (keyType == typeof(Guid))
            return Guid.Parse(keyString);

        // Add more key type conversions as needed
        return Convert.ChangeType(keyString, keyType);
    }

    /// <summary>
    /// Gets the default value for a given type.
    /// </summary>
    /// <param name="type">The type to get the default value for.</param>
    /// <returns>The default value for the type.</returns>
    private object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    /// <summary>
    /// Skips the current JSON value in the reader.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    private void SkipValue(ref JsonReader reader)
    {
        reader.SkipWhitespace();

        switch (reader.Current)
        {
            case (byte)'"':
                reader.ReadString();
                break;
            case (byte)'{':
                SkipObject(ref reader);
                break;
            case (byte)'[':
                SkipArray(ref reader);
                break;
            default:
                // Skip primitive value
                while (!reader.IsAtEnd && reader.Current != (byte)',' && reader.Current != (byte)'}' && reader.Current != (byte)']')
                {
                    reader.ReadNext();
                }
                break;
        }
    }

    /// <summary>
    /// Skips a JSON object in the reader.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    private void SkipObject(ref JsonReader reader)
    {
        reader.ReadNext(); // Skip '{'
        int depth = 1;

        while (!reader.IsAtEnd && depth > 0)
        {
            switch (reader.Current)
            {
                case (byte)'{':
                    depth++;
                    break;
                case (byte)'}':
                    depth--;
                    break;
                case (byte)'"':
                    reader.ReadString(); // Properly skip string to handle escaped quotes
                    continue;
            }
            reader.ReadNext();
        }
    }

    /// <summary>
    /// Skips a JSON array in the reader.
    /// </summary>
    /// <param name="reader">The JSON reader.</param>
    private void SkipArray(ref JsonReader reader)
    {
        reader.ReadNext(); // Skip '['
        int depth = 1;

        while (!reader.IsAtEnd && depth > 0)
        {
            switch (reader.Current)
            {
                case (byte)'[':
                    depth++;
                    break;
                case (byte)']':
                    depth--;
                    break;
                case (byte)'"':
                    reader.ReadString(); // Properly skip string
                    continue;
            }
            reader.ReadNext();
        }
    }

    /// <summary>
    /// Gets an array of serializable properties for a given type.
    /// </summary>
    /// <param name="type">The type to get properties for.</param>
    /// <returns>An array of <see cref="PropertyInfo"/> representing serializable properties.</returns>
    private PropertyInfo[] GetSerializableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !_config.IgnoreReadOnlyProperties || p.CanWrite)
            .ToArray();
    }

    /// <summary>
    /// Gets the property name based on the configured naming strategy.
    /// </summary>
    /// <param name="originalName">The original property name.</param>
    /// <returns>The formatted property name.</returns>
    private string GetPropertyName(string originalName)
    {
        return _config.NamingStrategy switch
        {
            NamingStrategy.CamelCase => ToCamelCase(originalName),
            NamingStrategy.SnakeCase => ToSnakeCase(originalName),
            NamingStrategy.KebabCase => ToKebabCase(originalName),
            _ => originalName
        };
    }

    /// <summary>
    /// Converts a string to camelCase.
    /// </summary>
    /// <param name="name">The string to convert.</param>
    /// <returns>The camelCase string.</returns>
    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    /// <summary>
    /// Converts a string to snake_case.
    /// </summary>
    /// <param name="name">The string to convert.</param>
    /// <returns>The snake_case string.</returns>
    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var result = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
                result.Append('_');
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    /// <summary>
    /// Converts a string to kebab-case.
    /// </summary>
    /// <param name="name">The string to convert.</param>
    /// <returns>The kebab-case string.</returns>
    private static string ToKebabCase(string name)
    {
        return ToSnakeCase(name).Replace('_', '-');
    }

    /// <summary>
    /// Tries to find a property in a dictionary, ignoring case if configured.
    /// </summary>
    /// <param name="properties">The dictionary of properties.</param>
    /// <param name="name">The name of the property to find.</param>
    /// <param name="property">When this method returns, contains the <see cref="PropertyInfo"/> if found; otherwise, null.</param>
    /// <returns><c>true</c> if the property was found; otherwise, <c>false</c>.</returns>
    private bool TryFindPropertyIgnoreCase(Dictionary<string, PropertyInfo> properties, string name, out PropertyInfo? property)
    {
        property = null;
        var match = properties.FirstOrDefault(kvp =>
            string.Equals(kvp.Key, name, StringComparison.OrdinalIgnoreCase));

        if (match.Key != null)
        {
            property = match.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to get a JSON converter for the specified type.
    /// </summary>
    /// <param name="type">The type to get the converter for.</param>
    /// <param name="converter">When this method returns, contains the <see cref="IJsonConverter"/> if found; otherwise, null.</param>
    /// <param name="property">Optional: The property being serialized/deserialized.</param>
    /// <returns><c>true</c> if a converter was found; otherwise, <c>false</c>.</returns>
    private bool TryGetConverter(Type type, out IJsonConverter? converter, PropertyInfo? property = null)
    {
        converter = null;

        // 1. Check for property-specific converters registered via fluent API
        if (property != null && _config.PropertyConverters.TryGetValue(property.DeclaringType!, out var propertyMap) &&
            propertyMap.TryGetValue(property.Name, out var propConverter))
        {
            converter = propConverter;
            return true;
        }

        // 2. Check for type-specific converters registered via fluent API
        if (_config.TypeConverters.TryGetValue(type, out converter))
        {
            return true;
        }

        // 3. Check for JsonConverterAttribute on the property
        if (property != null)
        {
            var propertyAttribute = property.GetCustomAttribute<JsonConverterAttribute>();
            if (propertyAttribute != null)
            {
                converter = (IJsonConverter)Activator.CreateInstance(propertyAttribute.ConverterType)!;
                return true;
            }
        }

        // 4. Check for JsonConverterAttribute on the type
        var typeAttribute = type.GetCustomAttribute<JsonConverterAttribute>();
        if (typeAttribute != null)
        {
            converter = (IJsonConverter)Activator.CreateInstance(typeAttribute.ConverterType)!;
            return true;
        }

        // 5. Check configured converters (and cache them)
        if (_converterCache.TryGetValue(type, out converter))
            return converter != null;

        converter = _config.Converters.FirstOrDefault(c => c.CanConvert(type));
        _converterCache[type] = converter;

        return converter != null;
    }

    /// <summary>
    /// Registers built-in JSON converters.
    /// </summary>
    private void RegisterBuiltInConverters()
    {
        _config.Converters.Add(new DateTimeConverter());
        _config.Converters.Add(new GuidConverter());
    }
}
