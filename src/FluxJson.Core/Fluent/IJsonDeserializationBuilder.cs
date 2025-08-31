// src/FluxJson.Core/Fluent/IJsonDeserializationBuilder.cs
namespace FluxJson.Core.Fluent;

/// <summary>
/// Fluent interface for JSON deserialization
/// </summary>
public interface IJsonDeserializationBuilder
{
    /// <summary>
    /// Configures deserialization options using a fluent builder.
    /// </summary>
    /// <param name="configure">An action to configure the JSON settings.</param>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder Configure(Action<IJsonConfigurationBuilder> configure);

    /// <summary>
    /// Applies a custom JSON configuration to the deserialization process.
    /// </summary>
    /// <param name="configuration">The custom JSON configuration.</param>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder WithConfiguration(JsonConfiguration configuration);

    /// <summary>
    /// Sets the naming strategy to camelCase for deserialization.
    /// </summary>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder UseCamelCase();

    /// <summary>
    /// Allows trailing commas in the JSON being deserialized.
    /// </summary>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder AllowTrailingCommas();

    /// <summary>
    /// Sets property name matching to be case-insensitive during deserialization.
    /// </summary>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder CaseInsensitive();

    /// <summary>
    /// Sets the performance mode for deserialization.
    /// </summary>
    /// <param name="mode">The performance mode to use.</param>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder WithPerformanceMode(PerformanceMode mode);

    /// <summary>
    /// Adds a custom JSON converter to be used during deserialization.
    /// </summary>
    /// <typeparam name="TConverter">The type of the converter to add.</typeparam>
    /// <returns>The current deserialization builder.</returns>
    IJsonDeserializationBuilder AddConverter<TConverter>() where TConverter : IJsonConverter, new();

    /// <summary>
    /// Deserializes the JSON to an object of the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    T? To<T>();

    /// <summary>
    /// Deserializes the JSON to an object of the specified type.
    /// </summary>
    /// <param name="type">The type to deserialize to.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    object? To(Type type);

    /// <summary>
    /// Deserializes JSON asynchronously from a stream to an object of the specified generic type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="stream">The stream containing the JSON data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous deserialization operation. The task result contains the deserialized object, or null if deserialization fails.</returns>
    Task<T?> FromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default);
}
