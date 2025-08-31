namespace FluxJson.Core.Converters;

/// <summary>
/// Interface for custom JSON converters
/// </summary>
public interface IJsonConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if this converter can convert the specified type; otherwise, false.</returns>
    bool CanConvert(Type type);

    /// <summary>
    /// Gets a value indicating whether this converter can read JSON.
    /// </summary>
    bool CanRead { get; }

    /// <summary>
    /// Gets a value indicating whether this converter can write JSON.
    /// </summary>
    bool CanWrite { get; }
}

/// <summary>
/// Generic JSON converter interface
/// </summary>
/// <typeparam name="T">The type to convert.</typeparam>
public interface IJsonConverter<T> : IJsonConverter
{
    /// <summary>
    /// Writes the JSON representation of the object.
    /// </summary>
    /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="configuration">The JSON configuration.</param>
    void Write(ref JsonWriter writer, T value, JsonConfiguration configuration);

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="configuration">The JSON configuration.</param>
    /// <returns>The object value.</returns>
    T? Read(ref JsonReader reader, JsonConfiguration configuration);
}
