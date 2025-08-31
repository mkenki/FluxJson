// src/FluxJson.Core/Fluent/JsonConfigurationBuilder.cs
using System.Text;

namespace FluxJson.Core.Fluent;

/// <summary>
/// Implementation of fluent configuration builder
/// </summary>
internal sealed class JsonConfigurationBuilder : IJsonConfigurationBuilder
{
    private readonly JsonConfiguration _config;

    internal JsonConfigurationBuilder(JsonConfiguration config)
    {
        _config = config.Clone();
    }

    /// <summary>
    /// Specifies the naming strategy for JSON properties.
    /// </summary>
    /// <param name="strategy">The naming strategy to use.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder UseNaming(NamingStrategy strategy)
    {
        _config.NamingStrategy = strategy;
        return this;
    }

    /// <summary>
    /// Specifies how null values are handled during serialization and deserialization.
    /// </summary>
    /// <param name="handling">The null handling strategy.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder HandleNulls(NullHandling handling)
    {
        _config.NullHandling = handling;
        return this;
    }

    /// <summary>
    /// Specifies the format for serializing and deserializing DateTime values.
    /// </summary>
    /// <param name="format">The DateTime format to use.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder FormatDates(DateTimeFormat format)
    {
        _config.DateTimeFormat = format;
        return this;
    }

    /// <summary>
    /// Specifies a custom format string for serializing and deserializing DateTime values.
    /// </summary>
    /// <param name="customFormat">The custom DateTime format string.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder FormatDates(string customFormat)
    {
        _config.DateTimeFormat = DateTimeFormat.Custom;
        _config.CustomDateTimeFormat = customFormat;
        return this;
    }

    /// <summary>
    /// Specifies the maximum depth allowed for JSON serialization and deserialization.
    /// </summary>
    /// <param name="depth">The maximum depth.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder WithMaxDepth(int depth)
    {
        _config.MaxDepth = depth;
        return this;
    }

    /// <summary>
    /// Specifies the encoding used for JSON serialization.
    /// </summary>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder WithEncoding(Encoding encoding)
    {
        _config.Encoding = encoding;
        return this;
    }

    /// <summary>
    /// Specifies whether the JSON output should be indented.
    /// </summary>
    /// <param name="indented">True to indent the output; otherwise, false. Default is true.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder WriteIndented(bool indented = true)
    {
        _config.WriteIndented = indented;
        return this;
    }

    /// <summary>
    /// Specifies whether trailing commas are allowed in JSON.
    /// </summary>
    /// <param name="allow">True to allow trailing commas; otherwise, false. Default is true.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder AllowTrailingCommas(bool allow = true)
    {
        _config.AllowTrailingCommas = allow;
        return this;
    }

    /// <summary>
    /// Specifies whether property name matching should be case-sensitive.
    /// </summary>
    /// <param name="sensitive">True for case-sensitive matching; otherwise, false. Default is true.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder CaseSensitive(bool sensitive = true)
    {
        _config.CaseSensitive = sensitive;
        return this;
    }

    /// <summary>
    /// Specifies the performance mode for the serializer.
    /// </summary>
    /// <param name="mode">The performance mode to use.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder WithPerformanceMode(PerformanceMode mode)
    {
        _config.PerformanceMode = mode;
        return this;
    }

    /// <summary>
    /// Adds a custom JSON converter to the configuration.
    /// </summary>
    /// <param name="converter">The converter to add.</param>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder AddConverter(IJsonConverter converter)
    {
        _config.Converters.Add(converter);
        return this;
    }

    /// <summary>
    /// Adds a custom JSON converter of a specified type to the configuration.
    /// </summary>
    /// <typeparam name="T">The type of the converter to add.</typeparam>
    /// <returns>The current configuration builder.</returns>
    public IJsonConfigurationBuilder AddConverter<T>() where T : IJsonConverter, new()
    {
        _config.Converters.Add(new T());
        return this;
    }

    /// <summary>
    /// Starts a fluent configuration for a specific type's converters.
    /// </summary>
    /// <typeparam name="T">The type to configure converters for.</typeparam>
    /// <returns>A fluent interface for registering type-specific or property-specific converters.</returns>
    public IJsonConverterRegistration<T> ForType<T>()
    {
        return new JsonConverterRegistration<T>(_config);
    }

    /// <summary>
    /// Builds the <see cref="JsonConfiguration"/> instance.
    /// </summary>
    /// <returns>A new <see cref="JsonConfiguration"/> instance.</returns>
    public JsonConfiguration Build()
    {
        return _config.Clone();
    }
}
