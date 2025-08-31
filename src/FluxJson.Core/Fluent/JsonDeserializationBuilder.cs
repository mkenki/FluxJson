// src/FluxJson.Core/Fluent/JsonDeserializationBuilder.cs
namespace FluxJson.Core.Fluent;

/// <summary>
/// Implementation of fluent deserialization builder
/// </summary>
internal sealed class JsonDeserializationBuilder : IJsonDeserializationBuilder
{
    private readonly ReadOnlyMemory<byte> _jsonBytes;
    private readonly JsonConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDeserializationBuilder"/> class with a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="config">The JSON configuration.</param>
    internal JsonDeserializationBuilder(string json, JsonConfiguration config)
    {
        _jsonBytes = config.Encoding.GetBytes(json);
        _config = config;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDeserializationBuilder"/> class with JSON bytes.
    /// </summary>
    /// <param name="json">The JSON bytes to deserialize.</param>
    /// <param name="config">The JSON configuration.</param>
    internal JsonDeserializationBuilder(ReadOnlySpan<byte> json, JsonConfiguration config)
    {
        _jsonBytes = json.ToArray();
        _config = config;
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder Configure(Action<IJsonConfigurationBuilder> configure)
    {
        var builder = new JsonConfigurationBuilder(_config);
        configure(builder);
        return new JsonDeserializationBuilder(_jsonBytes.Span, builder.Build());
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder WithConfiguration(JsonConfiguration configuration)
    {
        return new JsonDeserializationBuilder(_jsonBytes.Span, configuration);
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder UseCamelCase()
    {
        _config.NamingStrategy = NamingStrategy.CamelCase;
        return this;
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder AllowTrailingCommas()
    {
        _config.AllowTrailingCommas = true;
        return this;
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder CaseInsensitive()
    {
        _config.CaseSensitive = false;
        return this;
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder WithPerformanceMode(PerformanceMode mode)
    {
        _config.PerformanceMode = mode;
        return this;
    }

    /// <inheritdoc />
    public IJsonDeserializationBuilder AddConverter<TConverter>() where TConverter : IJsonConverter, new()
    {
        _config.Converters.Add(new TConverter());
        return this;
    }

    /// <inheritdoc />
    public T? To<T>()
    {
        var serializer = new FluxJsonSerializer(_config);
        return serializer.Deserialize<T>(_jsonBytes.Span);
    }

    /// <inheritdoc />
    public object? To(Type type)
    {
        var serializer = new FluxJsonSerializer(_config);
        return serializer.Deserialize(_config.Encoding.GetString(_jsonBytes.Span), type);
    }

    /// <inheritdoc />
    public async Task<T?> FromStreamAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        var serializer = new FluxJsonSerializer(_config);
        return await serializer.DeserializeAsync<T>(stream, cancellationToken);
    }
}
