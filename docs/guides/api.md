# API Reference 🔧

This comprehensive guide covers all public classes, interfaces, and methods available in FluxJson.

## Namespace
```csharp
using FluxJson.Core;
```

## Core Classes

### Json Class
The main entry point for FluxJson operations.

```csharp
public static class Json
{
    // Static methods for quick operations
    public static JsonBuilder<T> From<T>(T obj)
    public static JsonParser Parse(string json)
    public static string Serialize<T>(T obj)
    public static T Deserialize<T>(string json)
}
```

**Quick Start:**
```csharp
// Simple serialization
var person = new { Name = "John", Age = 30 };
string json = Json.Serialize(person);

// Fluent API usage
var result = Json.From(person)
    .Configure(config => config.UseNaming(NamingStrategy.CamelCase))
    .ToJson();
```

### FluxJsonSerializer Class
High-performance serializer with configuration support.

```csharp
public sealed class FluxJsonSerializer : IJsonSerializer
{
    // Constructors
    public FluxJsonSerializer(JsonConfiguration? configuration = null)

    // Serialization methods
    public string Serialize<T>(T value)
    public byte[] SerializeToBytes<T>(T value)
    public int SerializeToSpan<T>(T value, Span<byte> destination)
    public Task SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken)

    // Deserialization methods
    public T Deserialize<T>(string json)
    public T Deserialize<T>(ReadOnlySpan<byte> json)
    public T Deserialize<T>(Stream stream, CancellationToken cancellationToken)
}
```

**Usage:**
```csharp
var config = new JsonConfiguration {
    NamingStrategy = NamingStrategy.CamelCase,
    WriteIndented = true
};

var serializer = new FluxJsonSerializer(config);
string json = serializer.Serialize(myObject);
```

## Configuration Classes

### JsonConfiguration Class
Main configuration class for all serialization options.

```csharp
public class JsonConfiguration
{
    // Naming and formatting
    public NamingStrategy NamingStrategy { get; set; }
    public NullHandling NullHandling { get; set; }
    public DateTimeFormat DateTimeFormat { get; set; }
    public string CustomDateTimeFormat { get; set; }

    // Output options
    public bool WriteIndented { get; set; }
    public bool IgnoreReadOnlyProperties { get; set; }
    public bool CaseSensitive { get; set; }
    public bool AllowTrailingCommas { get; set; }

    // Performance
    public PerformanceMode PerformanceMode { get; set; }

    // Advanced
    public ICollection<IJsonConverter> Converters { get; }
    public Dictionary<Type, Dictionary<string, IJsonConverter>> PropertyConverters { get; }
    public Dictionary<Type, IJsonConverter> TypeConverters { get; }
}
```

### Configuration Builder (Fluent API)
```csharp
public interface IJsonConfigurationBuilder
{
    IJsonConfigurationBuilder UseNaming(NamingStrategy strategy);
    IJsonConfigurationBuilder HandleNulls(NullHandling handling);
    IJsonConfigurationBuilder FormatDates(DateTimeFormat format);
    IJsonConfigurationBuilder FormatDates(DateTimeFormat format, string customFormat);
    IJsonConfigurationBuilder WriteIndented(bool indented = true);
    IJsonConfigurationBuilder WithPerformanceMode(PerformanceMode mode);
    IJsonConfigurationBuilder IgnoreReadOnlyProperties(bool ignore = true);
}

// Usage: Available through Json.From().Configure()
Json.From(obj).Configure(config => config
    .UseNaming(NamingStrategy.CamelCase)
    .HandleNulls(NullHandling.Ignore)
    .WriteIndented()
    .WithPerformanceMode(PerformanceMode.Speed));
```

## Enumerations

### NamingStrategy
```csharp
public enum NamingStrategy
{
    Default,    // Uses original casing
    CamelCase,  // firstName
    PascalCase, // FirstName
    SnakeCase,  // first_name
    KebabCase   // first-name
}
```

### NullHandling
```csharp
public enum NullHandling
{
    Include,  // Include null values in JSON
    Ignore,   // Skip null properties
    Default   // Same as Include
}
```

### DateTimeFormat
```csharp
public enum DateTimeFormat
{
    Default,       // "\/Date(1693771168000)\/"
    ISO8601,       // "2023-09-03T21:19:28.000Z"
    UnixTimestamp, // 1693771168
    Custom         // Use CustomDateTimeFormat string
}
```

### PerformanceMode
```csharp
public enum PerformanceMode
{
    Speed,     // Maximum performance, limited features
    Balanced,  // Good balance of speed and features
    Features   // All features enabled, slightly slower
}
```

## Converter Interfaces

### IJsonConverter Interface
```csharp
public interface IJsonConverter
{
    bool CanConvert(Type type);

    // Serialization
    bool CanWrite { get; }
    void Write(ref JsonWriter writer, object value, JsonConfiguration config);

    // Deserialization
    bool CanRead { get; }
    object Read(ref JsonReader reader, Type type, JsonConfiguration config);
}
```

### Custom Converter Example
```csharp
public class GuidConverter : IJsonConverter {
    public bool CanConvert(Type type) => type == typeof(Guid);

    public bool CanWrite => true;
    public void Write(ref JsonWriter writer, object value, JsonConfiguration config) {
        writer.WriteValue(((Guid)value).ToString("N"));
    }

    public bool CanRead => true;
    public object Read(ref JsonReader reader, Type type, JsonConfiguration config) {
        var str = reader.ReadString();
        return Guid.TryParse(str, out var guid) ? guid : Guid.Empty;
    }
}
```

## Attributes

### JsonSerializableAttribute
Marks classes for source-generated serialization.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonSerializableAttribute : Attribute
{
    public JsonSerializableAttribute();
}
```

**Usage:**
```csharp
[JsonSerializable]
public class Product {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}
```

### JsonConverterAttribute
Specifies a custom converter for types or properties.

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property)]
public class JsonConverterAttribute : Attribute
{
    public JsonConverterAttribute(Type converterType);
    public Type ConverterType { get; }
}
```

## Extension Methods

### String Extensions
```csharp
public static class StringExtensions {
    // JSON parsing and manipulation helpers
}
```

### Configuration Extensions
```csharp
public static class JsonConfigurationExtensions {
    // Fluent configuration helpers
    public static JsonConfiguration WithNaming(this JsonConfiguration config, NamingStrategy strategy);
    public static JsonConfiguration WithIndented(this JsonConfiguration config);
    // ... other extension methods
}
```

## Exception Types

### JsonException
Base exception for all FluxJson operations.

```csharp
public class JsonException : Exception
{
    public JsonException(string message);
    public JsonException(string message, Exception innerException);
}
```

### JsonParseException
Thrown when JSON parsing fails.

```csharp
public class JsonParseException : JsonException
{
    public int Position { get; }
    public JsonParseException(string message, int position);
}
```

### JsonSerializationException
Thrown during serialization/deserialization errors.

```csharp
public class JsonSerializationException : JsonException
{
    public JsonSerializationException(string message);
    public JsonSerializationException(string message, Exception innerException);
}
```

## Parser Classes

### JsonParser
High-performance JSON parser.

```csharp
public class JsonParser {
    // Parsing methods
    public JsonParser(string json);
    public JsonParser(ReadOnlySpan<byte> jsonBytes);

    // Navigation and reading
    public bool Read();  // Move to next token
    public JsonTokenType TokenType { get; }
    public object? GetValue();

    // Value reading methods
    public string? ReadString();
    public int ReadInt32();
    public long ReadInt64();
    public double ReadDouble();
    public bool ReadBoolean();
}
```

### JsonWriter
Low-level JSON writer.

```csharp
public ref struct JsonWriter {
    public JsonWriter(Span<byte> buffer, JsonConfiguration config);

    // Writing methods
    public void WriteStartObject();
    public void WriteEndObject();
    public void WriteStartArray();
    public void WriteEndArray();
    public void WritePropertyName(string name);
    public void WriteValue(string value);
    public void WriteValue(int value);
    public void WriteValue(long value);
    public void WriteValue(double value);
    public void WriteValue(bool value);
    public void WriteNull();
    public void WriteSeparator();

    // Utility methods
    public void WriteNewLineAndIndent();
}
```

## Type Support Matrix

| Type Category | Support Level | Examples |
|---------------|---------------|----------|
| **Primitive Types** | ✅ Full | `int`, `string`, `bool`, `DateTime`, `Guid` |
| **Nullable Types** | ✅ Full | `int?`, `DateTime?`, `Guid?` |
| **Collections** | ✅ Full | `List<T>`, `T[]`, `Dictionary<TKey, TValue>` |
| **Custom Classes** | ✅ Full | User-defined classes and structs |
| **Enums** | ✅ Full | String representation |
| **Anonymous Objects** | ✅ Full | `new { Name = "John" }` |
| **Dynamic Objects** | ⚠️ Limited | Through dynamic typing |
| **Interfaces** | ❌ Not supported | Need concrete types |
| **Abstract Classes** | ❌ Not supported | Need concrete types |

## Performance Considerations

### Memory Efficiency
- Use `SerializeToSpan()` for zero-allocation scenarios
- Leverage buffer pooling for large objects
- Consider `PerformanceMode.Speed` for high-throughput applications

### Thread Safety
- `FluxJsonSerializer` instances are thread-safe
- Reuse instances across requests
- Configuration objects are immutable after creation

### Async Operations
```csharp
// Streaming for large data
using var fileStream = File.OpenRead("large-data.json");
var data = await serializer.DeserializeAsync<MyLargeObject>(fileStream);

// Writing to streams
using var outputStream = File.Create("output.json");
await serializer.SerializeAsync(myData, outputStream);
```

## Best Practices

### Configuration
- Create configuration once and reuse
- Use environment-specific configurations
- Prefer `PerformanceMode.Balanced` for most applications

### Error Handling
```csharp
try {
    var result = Json.Deserialize<MyType>(jsonString);
} catch (JsonParseException ex) {
    _logger.LogError("JSON parse error at position {Position}: {Message}", ex.Position, ex.Message);
} catch (JsonException ex) {
    _logger.LogError("JSON processing error: {Message}", ex.Message);
}
```

### Custom Converters
- Register converters globally via configuration
- Use property-specific converters when needed
- Implement both `Write` and `Read` for full roundtrip support

### Serialization Tips
- Use `[JsonSerializable]` attribute for performance-critical types
- Prefer typed deserialization over `dynamic`
- Handle null values explicitly or via configuration

See Also:
- [Getting Started with FluxJson](./getting-started.md)
- [Configuration Guide](./configuration.md)
- [Basic Examples](./basic-examples.md)
- [Web API Examples](./web-api-examples.md)
