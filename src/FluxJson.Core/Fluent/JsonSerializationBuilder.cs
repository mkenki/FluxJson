// src/FluxJson.Core/Fluent/JsonSerializationBuilder.cs
namespace FluxJson.Core.Fluent;

/// <summary>
/// Implementation of fluent serialization builder
/// </summary>
internal sealed class JsonSerializationBuilder<T> : IJsonSerializationBuilder<T>
{
    private readonly T _value;
    private readonly JsonConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSerializationBuilder{T}"/> class.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="config">The JSON configuration.</param>
    internal JsonSerializationBuilder(T value, JsonConfiguration config)
    {
        _value = value;
        _config = config;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> Configure(Action<IJsonConfigurationBuilder> configure)
    {
        var builder = new JsonConfigurationBuilder(_config);
        configure(builder);
        return new JsonSerializationBuilder<T>(_value, builder.Build());
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> WithConfiguration(JsonConfiguration configuration)
    {
        return new JsonSerializationBuilder<T>(_value, configuration);
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> UseCamelCase()
    {
        _config.NamingStrategy = NamingStrategy.CamelCase;
        return this;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> UseSnakeCase()
    {
        _config.NamingStrategy = NamingStrategy.SnakeCase;
        return this;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> IgnoreNulls()
    {
        _config.NullHandling = NullHandling.Ignore;
        return this;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> WriteIndented()
    {
        _config.WriteIndented = true;
        return this;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> WithPerformanceMode(PerformanceMode mode)
    {
        _config.PerformanceMode = mode;
        return this;
    }

    /// <inheritdoc />
    public IJsonSerializationBuilder<T> AddConverter<TConverter>() where TConverter : IJsonConverter, new()
    {
        _config.Converters.Add(new TConverter());
        return this;
    }

    /// <inheritdoc />
    public string ToJson()
    {
        // TODO: Implement actual serialization
        var serializer = new FluxJsonSerializer(_config);
        return serializer.Serialize(_value);
    }

    /// <inheritdoc />
    public byte[] ToBytes()
    {
        var json = ToJson();
        return _config.Encoding.GetBytes(json);
    }

    /// <inheritdoc />
    public int ToSpan(Span<byte> destination)
    {
        // TODO: Implement zero-allocation serialization to span
        var bytes = ToBytes();
        bytes.CopyTo(destination);
        return bytes.Length;
    }

    /// <inheritdoc />
    public async Task ToStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var serializer = new FluxJsonSerializer(_config);
        await serializer.SerializeAsync(_value, stream, cancellationToken);
    }
}
