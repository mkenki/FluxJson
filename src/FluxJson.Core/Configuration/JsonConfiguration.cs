// src/FluxJson.Core/Configuration/JsonConfiguration.cs
using System.Text;

namespace FluxJson.Core.Configuration;

/// <summary>
/// JSON serialization configuration
/// </summary>
public sealed class JsonConfiguration
{
    /// <summary>
    /// Gets or sets the naming strategy for JSON properties.
    /// </summary>
    public NamingStrategy NamingStrategy { get; set; } = NamingStrategy.Default;

    /// <summary>
    /// Gets or sets how null values are handled during serialization and deserialization.
    /// </summary>
    public NullHandling NullHandling { get; set; } = NullHandling.Include;

    /// <summary>
    /// Gets or sets the format for serializing and deserializing DateTime values.
    /// </summary>
    public DateTimeFormat DateTimeFormat { get; set; } = DateTimeFormat.Default;

    /// <summary>
    /// Gets or sets the custom format string for DateTime values when DateTimeFormat is Custom.
    /// </summary>
    public string? CustomDateTimeFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether read-only properties should be ignored during serialization.
    /// </summary>
    public bool IgnoreReadOnlyProperties { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the JSON output should be indented.
    /// </summary>
    public bool WriteIndented { get; set; } = false;

    /// <summary>
    /// Gets or sets the encoding used for JSON serialization.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Gets or sets the maximum depth allowed for JSON serialization and deserialization.
    /// </summary>
    public int MaxDepth { get; set; } = 64;

    /// <summary>
    /// Gets or sets a value indicating whether trailing commas are allowed in JSON.
    /// </summary>
    public bool AllowTrailingCommas { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether property name matching should be case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <summary>
    /// Performance mode settings
    /// </summary>
    public PerformanceMode PerformanceMode { get; set; } = PerformanceMode.Balanced;

    /// <summary>
    /// Custom converters
    /// </summary>
    public List<IJsonConverter> Converters { get; set; } = new();

    /// <summary>
    /// Gets a dictionary of type-specific converters.
    /// Key: Type, Value: IJsonConverter
    /// </summary>
    internal Dictionary<Type, IJsonConverter> TypeConverters { get; } = new();

    /// <summary>
    /// Gets a dictionary of property-specific converters, where the outer key is the type and the inner dictionary maps property names to their respective converters.
    /// </summary>
    internal Dictionary<Type, Dictionary<string, IJsonConverter>> PropertyConverters { get; } = new();

    /// <summary>
    /// Registers a custom converter for a specific type.
    /// </summary>
    /// <param name="type">The type to register the converter for.</param>
    /// <param name="converter">The converter instance.</param>
    internal void RegisterTypeConverter(Type type, IJsonConverter converter)
    {
        TypeConverters[type] = converter;
    }

    /// <summary>
    /// Registers a custom converter for a specific property of a type.
    /// </summary>
    /// <param name="type">The type containing the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="converter">The converter instance.</param>
    internal void RegisterPropertyConverter(Type type, string propertyName, IJsonConverter converter)
    {
        if (!PropertyConverters.TryGetValue(type, out var propertyMap))
        {
            propertyMap = new Dictionary<string, IJsonConverter>();
            PropertyConverters[type] = propertyMap;
        }
        propertyMap[propertyName] = converter;
    }

    /// <summary>
    /// Create a copy of current configuration
    /// </summary>
    public JsonConfiguration Clone()
    {
        var clone = new JsonConfiguration
        {
            NamingStrategy = NamingStrategy,
            NullHandling = NullHandling,
            DateTimeFormat = DateTimeFormat,
            CustomDateTimeFormat = CustomDateTimeFormat,
            IgnoreReadOnlyProperties = IgnoreReadOnlyProperties,
            WriteIndented = WriteIndented,
            Encoding = Encoding,
            MaxDepth = MaxDepth,
            AllowTrailingCommas = AllowTrailingCommas,
            CaseSensitive = CaseSensitive,
            PerformanceMode = PerformanceMode,
            Converters = new List<IJsonConverter>(Converters)
        };

        // Clone type and property converters
        foreach (var entry in TypeConverters)
        {
            clone.TypeConverters.Add(entry.Key, entry.Value);
        }
        foreach (var entry in PropertyConverters)
        {
            clone.PropertyConverters.Add(entry.Key, new Dictionary<string, IJsonConverter>(entry.Value));
        }
        return clone;
    }
}
