namespace FluxJson.Core.Configuration;

/// <summary>
/// Defines the naming strategies for JSON properties.
/// </summary>
public enum NamingStrategy
{
    /// <summary>
    /// Uses the default naming as defined in the object.
    /// </summary>
    Default,
    /// <summary>
    /// Converts property names to camelCase (e.g., "propertyName").
    /// </summary>
    CamelCase,
    /// <summary>
    /// Converts property names to PascalCase (e.g., "PropertyName").
    /// </summary>
    PascalCase,
    /// <summary>
    /// Converts property names to snake_case (e.g., "property_name").
    /// </summary>
    SnakeCase,
    /// <summary>
    /// Converts property names to kebab-case (e.g., "property-name").
    /// </summary>
    KebabCase
}

/// <summary>
/// Defines how null values are handled during serialization and deserialization.
/// </summary>
public enum NullHandling
{
    /// <summary>
    /// Includes null values in the JSON output.
    /// </summary>
    Include,
    /// <summary>
    /// Ignores null values, excluding them from the JSON output.
    /// </summary>
    Ignore,
    /// <summary>
    /// Uses the default null handling behavior.
    /// </summary>
    Default
}

/// <summary>
/// Defines the format for serializing and deserializing DateTime values.
/// </summary>
public enum DateTimeFormat
{
    /// <summary>
    /// Uses the default DateTime format.
    /// </summary>
    Default,
    /// <summary>
    /// Formats DateTime values according to ISO 8601 standard.
    /// </summary>
    ISO8601,
    /// <summary>
    /// Formats DateTime values as Unix timestamps.
    /// </summary>
    UnixTimestamp,
    /// <summary>
    /// Allows for a custom DateTime format string to be specified.
    /// </summary>
    Custom
}

/// <summary>
/// Defines different performance modes for the serializer.
/// </summary>
public enum PerformanceMode
{
    /// <summary>
    /// Prioritizes maximum performance with minimal features.
    /// </summary>
    Speed,
    /// <summary>
    /// Balances between speed and available features.
    /// </summary>
    Balanced,
    /// <summary>
    /// Enables all features, potentially at the cost of performance.
    /// </summary>
    Features
}

/// <summary>
/// Specifies the type of JSON token.
/// </summary>
public enum JsonTokenType
{
    /// <summary>
    /// No token has been read yet.
    /// </summary>
    None,
    /// <summary>
    /// The start of a JSON object ('{').
    /// </summary>
    StartObject,
    /// <summary>
    /// The end of a JSON object ('}').
    /// </summary>
    EndObject,
    /// <summary>
    /// The start of a JSON array ('[').
    /// </summary>
    StartArray,
    /// <summary>
    /// The end of a JSON array (']').
    /// </summary>
    EndArray,
    /// <summary>
    /// A JSON property name.
    /// </summary>
    PropertyName,
    /// <summary>
    /// A JSON string value.
    /// </summary>
    String,
    /// <summary>
    /// A JSON number value.
    /// </summary>
    Number,
    /// <summary>
    /// A JSON boolean value (true or false).
    /// </summary>
    True,
    /// <summary>
    /// A JSON boolean value (true or false).
    /// </summary>
    False,
    /// <summary>
    /// A JSON null value.
    /// </summary>
    Null,
    /// <summary>
    /// A comma (',') separator.
    /// </summary>
    Comma,
    /// <summary>
    /// A colon (':') separator.
    /// </summary>
    Colon,
    /// <summary>
    /// The end of the JSON data.
    /// </summary>
    EndOfDocument
}
