# Json Class 📋

The `Json` class is the main entry point for FluxJson serialization and deserialization operations. It provides static methods for common JSON operations and fluent API builders for advanced usage.

## Namespace
```csharp
using FluxJson.Core;
```

## Static Methods

### From<T>(T obj)
Creates a `JsonBuilder<T>` instance for fluent API usage with the specified object.

```csharp
public static JsonBuilder<T> From<T>(T obj)
```

**Parameters:**
- `T obj`: The object to serialize

**Returns:** `JsonBuilder<T>` - A fluent builder for configuring and executing JSON operations

**Example:**
```csharp
var person = new Person { Name = "Alice", Age = 30 };
var jsonBuilder = Json.From(person);
```

### Parse(string json)
Creates a `JsonParser` instance for parsing JSON strings.

```csharp
public static JsonParser Parse(string json)
```

**Parameters:**
- `string json`: The JSON string to parse

**Returns:** `JsonParser` - A parser instance for extracting values

**Example:**
```csharp
var parser = Json.Parse("{\"name\":\"Alice\",\"age\":30}");
var name = parser["name"].AsString();
```

### Parse(ReadOnlySpan<byte> jsonBytes)
Creates a `JsonParser` instance for parsing JSON byte data.

```csharp
public static JsonParser Parse(ReadOnlySpan<byte> jsonBytes)
```

**Parameters:**
- `ReadOnlySpan<byte> jsonBytes`: The JSON byte data to parse

**Returns:** `JsonParser` - A parser instance for extracting values

**Usage:**
```csharp
ReadOnlySpan<byte> jsonBytes = Encoding.UTF8.GetBytes(jsonString);
var parser = Json.Parse(jsonBytes);
```

### Serialize<T>(T obj)
Quick serialization method that converts an object to a JSON string with default settings.

```csharp
public static string Serialize<T>(T obj)
```

**Parameters:**
- `T obj`: The object to serialize

**Returns:** `string` - JSON representation of the object

**Example:**
```csharp
var person = new Person { Name = "Alice", Age = 30 };
string json = Json.Serialize(person);
// {"Name":"Alice","Age":30}
```

### Deserialize<T>(string json)
Quick deserialization method that converts a JSON string to an object of specified type.

```csharp
public static T Deserialize<T>(string json)
```

**Parameters:**
- `string json`: The JSON string to deserialize

**Returns:** `T` - Deserialized object of the specified type

**Example:**
```csharp
string json = "{\"Name\":\"Alice\",\"Age\":30}";
Person person = Json.Deserialize<Person>(json);
```

## Internal Methods

### IsSourceGeneratedType<T>()
Determines if a type is source-generated for optimized serialization.

```csharp
private static bool IsSourceGeneratedType<T>()
```

**Returns:** `bool` - True if the type has `FluxJsonGeneratedAttribute`

### IsAnonymousType(Type type)
Helper method to detect anonymous types.

```csharp
private static bool IsAnonymousType(Type type)
```

**Parameters:**
- `Type type`: The type to check

**Returns:** `bool` - True if the type is anonymous

## Usage Patterns

### Basic Usage
```csharp
using FluxJson;

var user = new User { Id = 1, Name = "John" };

// Simple serialization
string json = Json.Serialize(user);

// Simple deserialization
User deserializedUser = Json.Deserialize<User>(json);
```

### Fluent API
```csharp
var result = Json.From(user)
    .Configure(config => config
        .UseNaming(NamingStrategy.CamelCase)
        .WriteIndented(true)
        .WithPerformanceMode(PerformanceMode.Speed))
    .ToJson();
```

### JsonParser Usage
```csharp
var json = "{\"users\":[{\"name\":\"Alice\"},{\"name\":\"Bob\"}]}";
var parser = Json.Parse(json);

var users = parser["users"].AsArray();
foreach (var user in users)
{
    Console.WriteLine($"User: {user["name"].AsString()}");
}
```

### Byte-Based Parsing
```csharp
using System.Text;

var jsonBytes = Encoding.UTF8.GetBytes("{\"key\":\"value\"}");
var parser = Json.Parse(jsonBytes);
var value = parser["key"].AsString();
```

## Performance Considerations

- Use `IsSourceGeneratedType<T>()` for automatic optimization detection
- Anonymous types fall back to reflection-based serialization
- Byte-based parsing is recommended for high-performance scenarios

## Thread Safety

All methods are thread-safe and can be used in concurrent environments.

## Exceptions

May throw:
- `JsonException` - For general JSON processing errors
- `JsonParseException` - For JSON parsing errors
- `JsonSerializationException` - For serialization-related errors

## See Also

- [JsonBuilder<T>](./JsonBuilder.md) - Fluent API builder
- [JsonParser](./JsonParser.md) - JSON parsing engine
- [JsonConfiguration](./JsonConfiguration.md) - Configuration options
