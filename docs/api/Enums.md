# Enumeration Types 📚

This page documents all enumeration types used throughout FluxJson for configuration and token parsing.

## NamingStrategy

Defines the naming strategies for JSON properties during serialization and deserialization.

```csharp
public enum NamingStrategy
{
    Default,    // Uses the default naming as defined in the object
    CamelCase,  // e.g., "propertyName"
    PascalCase, // e.g., "PropertyName"
    SnakeCase,  // e.g., "property_name"
    KebabCase   // e.g., "property-name"
}
```

### Usage
```csharp
// Configure camelCase naming
var json = Json.From(person)
    .Configure(config => config.UseNaming(NamingStrategy.CamelCase))
    .ToJson();

// Result: {"firstName":"John","lastName":"Doe"}
```

### Transformation Examples

| Original | CamelCase | PascalCase | SnakeCase | KebabCase |
|----------|-----------|------------|-----------|-----------|
| FirstName | firstName | FirstName | first_name | first-name |
| UserID | userID | UserID | user_id | user-id |
| IsActive | isActive | IsActive | is_active | is-active |

## NullHandling

Defines how null values are handled during serialization.

```csharp
public enum NullHandling
{
    Include,  // Include null values in JSON output
    Ignore,   // Skip null properties entirely
    Default   // Use default behavior
}
```

### Usage
```csharp
public class User {
    public string Name { get; set; }      // "John"
    public string Email { get; set; }     // null
    public int? Age { get; set; }         // null
}

// Include nulls (default)
Json.From(user).Configure(c => c.HandleNulls(NullHandling.Include))
// {"Name":"John","Email":null,"Age":null}

// Ignore nulls
Json.From(user).Configure(c => c.HandleNulls(NullHandling.Ignore))
// {"Name":"John"}
```

## DateTimeFormat

Defines the format for serializing and deserializing DateTime values.

```csharp
public enum DateTimeFormat
{
    Default,       // Use default DateTime format
    ISO8601,       // 2023-09-03T21:19:28.000Z
    UnixTimestamp, // 1693777168
    Custom         // Use custom format string
}
```

### Usage
```csharp
var date = new DateTime(2023, 9, 3, 21, 19, 28);

// ISO 8601 format
Json.From(date).Configure(c => c.FormatDates(DateTimeFormat.ISO8601))
// "2023-09-03T21:19:28.000Z"

// Unix timestamp
Json.From(date).Configure(c => c.FormatDates(DateTimeFormat.UnixTimestamp))
// 1693777168

// Custom format
Json.From(date).Configure(c => c.FormatDates(DateTimeFormat.Custom, "yyyy-MM-dd"))
// "2023-09-03"
```

### Format Examples

| Format | Example Output |
|--------|----------------|
| ISO8601 | 2023-09-03T21:19:28.000Z |
| UnixTimestamp | 1693777168 |
| Custom("yyyy-MM-dd") | 2023-09-03 |
| Default | DateTime as string |

## PerformanceMode

Defines different performance modes for the serializer, balancing speed vs features.

```csharp
public enum PerformanceMode
{
    Speed,     // Maximum performance, minimal features
    Balanced,  // Balance between speed and features (default)
    Features   // Enable all features, slightly slower
}
```

### Usage
```csharp
// Speed mode - fastest serialization
Json.From(data).Configure(c => c.WithPerformanceMode(PerformanceMode.Speed))

// Balanced mode - default, good balance
Json.From(data).Configure(c => c.WithPerformanceMode(PerformanceMode.Balanced))

// Features mode - all features enabled
Json.From(data).Configure(c => c.WithPerformanceMode(PerformanceMode.Features))
```

### Performance Characteristics

| Mode | Speed | Memory Usage | Features Available |
|------|-------|--------------|-------------------|
| Speed | ⚡ Fastest | 🔸 Lowest | Limited |
| Balanced | ⚡ Fast | 🔸 Moderate | Most features |
| Features | 🐌 Slower | 🔸 Highest | All features |

#### Speed Mode Limitations
- Limited custom converter support
- No advanced validation
- Minimal error reporting
- Basic null handling

#### Features Mode Benefits
- Full custom converter support
- Extensive validation
- Detailed error reporting
- Advanced formatting options

## JsonTokenType

Internal enumeration for JSON token types used by the parser.

```csharp
public enum JsonTokenType
{
    None,           // No token read yet
    StartObject,    // '{'
    EndObject,      // '}'
    StartArray,     // '['
    EndArray,       // ']'
    PropertyName,   // String used as property name
    String,         // String value
    Number,         // Numeric value
    True,           // Boolean true
    False,          // Boolean false
    Null,           // Null value
    Comma,          // ','
    Colon,          // ':'
    EndOfDocument   // End of JSON data
}
```

### Usage in Parser
```csharp
var parser = Json.Parse(jsonString);
parser.Read(); // Advances to next token

switch (parser.TokenType) {
    case JsonTokenType.StartObject:
        // Handle object start
        break;
    case JsonTokenType.PropertyName:
        var propertyName = parser.GetString();
        break;
}
```

### Token Flow Example
```json
{"name":"John","age":30}
```

Token sequence:
1. `StartObject` ('{')
2. `PropertyName` ("name")
3. `String` ("John")
4. `PropertyName` ("age")
5. `Number` (30)
6. `EndObject` ('}')

## Best Practices

### Choosing Naming Strategy
- Use `CamelCase` for JavaScript/TypeScript APIs
- Use `SnakeCase` for Python/Ruby APIs
- Use `KebabCase` for URL-friendly APIs
- Use `PascalCase` for .NET/Java APIs

### Performance Mode Selection
```csharp
// For high-throughput applications
PerformanceMode.Speed

// For most applications (recommended)
PerformanceMode.Balanced

// For complex scenarios needing all features
PerformanceMode.Features
```

### Null Handling
```csharp
// APIs that need precise null representation
NullHandling.Include

// Cleaner APIs that hide null values
NullHandling.Ignore
```

### DateTime Format
```csharp
// Web APIs and modern applications
DateTimeFormat.ISO8601

// Legacy systems integration
DateTimeFormat.UnixTimestamp

// Custom display requirements
DateTimeFormat.Custom
```

## See Also

- [JsonConfiguration](../api/JsonConfiguration.md) - Main configuration class
- [Json](../../api/Json.md) - Main entry point for serialization
- [Configuration Guide](../../guides/configuration.md) - Detailed configuration examples
