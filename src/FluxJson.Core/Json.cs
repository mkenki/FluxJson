// src/FluxJson.Core/Json.cs
using FluxJson.Core.Configuration;
using FluxJson.Core.Fluent;
using FluxJson.Core.Serialization; // Added for JsonSerializationLogic

namespace FluxJson;

/// <summary>
/// Main entry point for FluxJson serialization
/// </summary>
public static class Json
{
    /// <summary>
    /// The default JSON configuration used by the static Json methods.
    /// </summary>
    private static readonly JsonConfiguration DefaultConfiguration = new();

    /// <summary>
    /// Creates a serialization pipeline from an object.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>An <see cref="IJsonSerializationBuilder{T}"/> instance for fluent serialization configuration.</returns>
    public static IJsonSerializationBuilder<T> From<T>(T value)
    {
        return new JsonSerializationBuilder<T>(value, DefaultConfiguration.Clone());
    }

    /// <summary>
    /// Creates a deserialization pipeline from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>An <see cref="IJsonDeserializationBuilder"/> instance for fluent deserialization configuration.</returns>
    public static IJsonDeserializationBuilder Parse(string json)
    {
        return new JsonDeserializationBuilder(json, DefaultConfiguration.Clone());
    }

    /// <summary>
    /// Creates a deserialization pipeline from JSON bytes.
    /// </summary>
    /// <param name="json">The JSON bytes to parse.</param>
    /// <returns>An <see cref="IJsonDeserializationBuilder"/> instance for fluent deserialization configuration.</returns>
    public static IJsonDeserializationBuilder Parse(ReadOnlySpan<byte> json)
    {
        return new JsonDeserializationBuilder(json, DefaultConfiguration.Clone());
    }

    /// <summary>
    /// Creates a configuration pipeline for customizing JSON serialization/deserialization settings.
    /// </summary>
    /// <returns>An <see cref="IJsonConfigurationBuilder"/> instance for fluent configuration.</returns>
    public static IJsonConfigurationBuilder Configure()
    {
        return new JsonConfigurationBuilder(DefaultConfiguration.Clone());
    }

    /// <summary>
    /// Quickly serializes an object to a JSON string using default settings.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string Serialize<T>(T value)
    {
        return From(value).ToJson();
    }

    /// <summary>
    /// Quickly deserializes a JSON string to an object of the specified type using default settings.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized object, or default value if deserialization fails.</returns>
    public static T? Deserialize<T>(string json)
    {
        return Parse(json).To<T>();
    }
    /// <summary>
    /// Deserializes JSON using the provided JsonReader into an object of type T that implements IJsonSerializable.
    /// </summary>
    /// <typeparam name="T">The type that implements IJsonSerializable to deserialize into.</typeparam>
    /// <param name="reader">The JsonReader instance containing the JSON to deserialize.</param>
    /// <returns>A deserialized instance of type T.</returns>
    public static T FromJson<T>(JsonReader reader) where T : IJsonSerializable<T>
    {
        return T.FromJson(reader);
    }


    /// <summary>
    /// Deserializes JSON bytes into an object of type T that implements IJsonSerializable.
    /// </summary>
    /// <typeparam name="T">The type that implements IJsonSerializable to deserialize into.</typeparam>
    /// <param name="json">The JSON bytes to deserialize.</param>
    /// <returns>A deserialized instance of type T.</returns>
    public static T FromJson<T>(ReadOnlySpan<byte> json) where T : IJsonSerializable<T>
    {
        var reader = new JsonReader(json, DefaultConfiguration);
        return T.FromJson(reader);
    }

    /// <summary>
    /// Helper method for source generator to serialize IJsonSerializable objects.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="value">The object to serialize.</param>
    /// <param name="writer">The JsonWriter instance to write to.</param>
    public static void ToJson<T>(T value, ref JsonWriter writer) where T : IJsonSerializable<T>
    {
        value.ToJson(ref writer);
    }

    /// <summary>
    /// Helper method for source generator to deserialize IJsonSerializable objects.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="reader">The JsonReader instance to read from.</param>
    /// <returns>The deserialized object.</returns>
    public static T FromJsonSerializable<T>(JsonReader reader) where T : IJsonSerializable<T>
    {
        return T.FromJson(reader);
    }
}
