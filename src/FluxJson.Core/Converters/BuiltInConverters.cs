// src/FluxJson.Core/Converters/BuiltInConverters.cs
namespace FluxJson.Core.Converters;

/// <summary>
/// Converts <see cref="DateTime"/> values to and from JSON.
/// </summary>
public sealed class DateTimeConverter : IJsonConverter<DateTime>
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted; otherwise, false.</returns>
    public bool CanConvert(Type type) => type == typeof(DateTime);

    /// <summary>
    /// Gets a value indicating whether this converter can read JSON.
    /// </summary>
    public bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON.
    /// </summary>
    public bool CanWrite => true;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="config">The JSON configuration.</param>
    public void Write(ref JsonWriter writer, object? value, JsonConfiguration config)
    {
        if (value is DateTime dateTimeValue)
        {
            Write(ref writer, dateTimeValue, config);
        }
        else
        {
            throw new ArgumentException($"Expected DateTime value, but got {value?.GetType().Name ?? "null"}");
        }
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="type">The type to deserialize.</param>
    /// <param name="config">The JSON configuration.</param>
    /// <returns>The object value.</returns>
    public object? Read(ref JsonReader reader, Type type, JsonConfiguration config)
    {
        return Read(ref reader, config);
    }

    /// <summary>
    /// Writes the JSON representation of the <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="DateTime"/> value to write.</param>
    /// <param name="configuration">The JSON configuration.</param>
    public void Write(ref JsonWriter writer, DateTime value, JsonConfiguration configuration)
    {
        var formatted = configuration.DateTimeFormat switch
        {
            DateTimeFormat.ISO8601 => value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            DateTimeFormat.UnixTimestamp => ((DateTimeOffset)value).ToUnixTimeSeconds().ToString(),
            _ => value.ToString("O") // Default ISO 8601
        };

        writer.WriteValue(formatted);
    }

    /// <summary>
    /// Reads the JSON representation of a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="configuration">The JSON configuration.</param>
    /// <returns>The deserialized <see cref="DateTime"/> value.</returns>
    public DateTime Read(ref JsonReader reader, JsonConfiguration configuration)
    {
        var dateString = reader.ReadString();
        if (dateString is null)
            throw new JsonException("Expected date string");

        return configuration.DateTimeFormat switch
        {
            DateTimeFormat.UnixTimestamp when long.TryParse(dateString, out var timestamp)
                => DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime,
            _ => DateTime.Parse(dateString)
        };
    }
}

/// <summary>
/// Converts <see cref="Guid"/> values to and from JSON.
/// </summary>
public sealed class GuidConverter : IJsonConverter<Guid>
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type can be converted; otherwise, false.</returns>
    public bool CanConvert(Type type) => type == typeof(Guid);

    /// <summary>
    /// Gets a value indicating whether this converter can read JSON.
    /// </summary>
    public bool CanRead => true;

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON.
    /// </summary>
    public bool CanWrite => true;

    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="config">The JSON configuration.</param>
    public void Write(ref JsonWriter writer, object? value, JsonConfiguration config)
    {
        if (value is Guid guidValue)
        {
            Write(ref writer, guidValue, config);
        }
        else
        {
            throw new ArgumentException($"Expected Guid value, but got {value?.GetType().Name ?? "null"}");
        }
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="type">The type to deserialize.</param>
    /// <param name="config">The JSON configuration.</param>
    /// <returns>The object value.</returns>
    public object? Read(ref JsonReader reader, Type type, JsonConfiguration config)
    {
        return Read(ref reader, config);
    }

    /// <summary>
    /// Writes the JSON representation of the <see cref="Guid"/> object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The <see cref="Guid"/> value to write.</param>
    /// <param name="configuration">The JSON configuration.</param>
    public void Write(ref JsonWriter writer, Guid value, JsonConfiguration configuration)
    {
        writer.WriteValue(value.ToString());
    }

    /// <summary>
    /// Reads the JSON representation of a <see cref="Guid"/> object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="configuration">The JSON configuration.</param>
    /// <returns>The deserialized <see cref="Guid"/> value.</returns>
    public Guid Read(ref JsonReader reader, JsonConfiguration configuration)
    {
        var guidString = reader.ReadString();
        if (guidString is null)
            throw new JsonException("Expected GUID string");

        if (!Guid.TryParse(guidString, out var guid))
            throw new JsonException($"Invalid GUID format: {guidString}");

        return guid;
    }
}
