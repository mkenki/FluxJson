namespace FluxJson.Core.Fluent;

/// <summary>
/// Fluent interface for JSON serialization
/// </summary>
public interface IJsonSerializationBuilder<T>
{
    /// <summary>
    /// Configures serialization options using a fluent builder.
    /// </summary>
    /// <param name="configure">An action to configure the JSON settings.</param>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> Configure(Action<IJsonConfigurationBuilder> configure);

    /// <summary>
    /// Applies a custom JSON configuration to the serialization process.
    /// </summary>
    /// <param name="configuration">The custom JSON configuration.</param>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> WithConfiguration(JsonConfiguration configuration);

    /// <summary>
    /// Sets the naming strategy to camelCase for serialization.
    /// </summary>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> UseCamelCase();

    /// <summary>
    /// Sets the naming strategy to snake_case for serialization.
    /// </summary>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> UseSnakeCase();

    /// <summary>
    /// Ignores null values during serialization.
    /// </summary>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> IgnoreNulls();

    /// <summary>
    /// Writes the JSON output with indentation.
    /// </summary>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> WriteIndented();

    /// <summary>
    /// Sets the performance mode for serialization.
    /// </summary>
    /// <param name="mode">The performance mode to use.</param>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> WithPerformanceMode(PerformanceMode mode);

    /// <summary>
    /// Adds a custom JSON converter to be used during serialization.
    /// </summary>
    /// <typeparam name="TConverter">The type of the converter to add.</typeparam>
    /// <returns>The current serialization builder.</returns>
    IJsonSerializationBuilder<T> AddConverter<TConverter>() where TConverter : IJsonConverter, new();

    /// <summary>
    /// Serializes the object to a JSON string.
    /// </summary>
    /// <returns>The JSON string representation of the object.</returns>
    string ToJson();

    /// <summary>
    /// Serializes the object to a JSON byte array.
    /// </summary>
    /// <returns>The JSON byte array representation of the object.</returns>
    byte[] ToBytes();

    /// <summary>
    /// Serializes the object to a JSON byte span, minimizing allocations.
    /// </summary>
    /// <param name="destination">The destination span to write the JSON bytes to.</param>
    /// <returns>The number of bytes written to the destination span.</returns>
    int ToSpan(Span<byte> destination);

    /// <summary>
    /// Serializes the object to a stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write the JSON data to.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    Task ToStreamAsync(Stream stream, CancellationToken cancellationToken = default);
}
