// src/FluxJson.Core/Fluent/IJsonConfigurationBuilder.cs
using System.Text;

namespace FluxJson.Core.Fluent;

/// <summary>
/// Fluent interface for JSON configuration
/// </summary>
public interface IJsonConfigurationBuilder
{
    /// <summary>
    /// Set naming strategy
    /// </summary>
    IJsonConfigurationBuilder UseNaming(NamingStrategy strategy);

    /// <summary>
    /// Set null handling
    /// </summary>
    IJsonConfigurationBuilder HandleNulls(NullHandling handling);

    /// <summary>
    /// Set date time format
    /// </summary>
    IJsonConfigurationBuilder FormatDates(DateTimeFormat format);

    /// <summary>
    /// Set custom date time format
    /// </summary>
    IJsonConfigurationBuilder FormatDates(string customFormat);

    /// <summary>
    /// Set maximum depth
    /// </summary>
    IJsonConfigurationBuilder WithMaxDepth(int depth);

    /// <summary>
    /// Set encoding
    /// </summary>
    IJsonConfigurationBuilder WithEncoding(Encoding encoding);

    /// <summary>
    /// Enable/disable indented writing
    /// </summary>
    IJsonConfigurationBuilder WriteIndented(bool indented = true);

    /// <summary>
    /// Enable/disable trailing commas
    /// </summary>
    IJsonConfigurationBuilder AllowTrailingCommas(bool allow = true);

    /// <summary>
    /// Set case sensitivity
    /// </summary>
    IJsonConfigurationBuilder CaseSensitive(bool sensitive = true);

    /// <summary>
    /// Set performance mode
    /// </summary>
    IJsonConfigurationBuilder WithPerformanceMode(PerformanceMode mode);

    /// <summary>
    /// Add custom converter
    /// </summary>
    IJsonConfigurationBuilder AddConverter(IJsonConverter converter);

    /// <summary>
    /// Add custom converter by type
    /// </summary>
    IJsonConfigurationBuilder AddConverter<T>() where T : IJsonConverter, new();

    /// <summary>
    /// Starts a fluent configuration for a specific type's converters.
    /// </summary>
    /// <typeparam name="T">The type to configure converters for.</typeparam>
    /// <returns>A fluent interface for registering type-specific or property-specific converters.</returns>
    IJsonConverterRegistration<T> ForType<T>();

    /// <summary>
    /// Build configuration
    /// </summary>
    JsonConfiguration Build();
}
