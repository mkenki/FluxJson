# FluxJsonSerializer Class 🔧

The `FluxJsonSerializer` class is the core serialization engine of FluxJson. It provides high-performance JSON serialization and deserialization with extensive configuration options, custom converters, and performance optimizations.

## Namespace
```csharp
using FluxJson.Core;
```

## Inheritance
```csharp
public sealed class FluxJsonSerializer : IJsonSerializer
```

## Constructors

### FluxJsonSerializer()
Default constructor using default configuration.

```csharp
public FluxJsonSerializer()
```

**Usage:**
```csharp
var serializer = new FluxJsonSerializer();
```

### FluxJsonSerializer(JsonConfiguration configuration)
Constructor with custom configuration.

```csharp
public FluxJsonSerializer(JsonConfiguration configuration)
```

**Parameters:**
- `configuration`: JSON configuration instance (optional, defaults to JsonConfiguration.Default)

**Example:**
```csharp
var config = new JsonConfiguration {
    NamingStrategy = NamingStrategy.CamelCase,
    WriteIndented = true
};
var serializer = new FluxJsonSerializer(config);
```

## Serialization Methods

### Serialize<T>(T value)
Serializes an object to a JSON string.

```csharp
public string Serialize<T>(T value)
```

**Type Parameters:**
- `T`: Type of object to serialize

**Parameters:**
- `value`: Object to serialize

**Returns:** `string` - JSON representation

**Example:**
```csharp
var person = new Person { Name = "Alice", Age = 30 };
string json = serializer.Serialize(person);
// {"Name":"Alice","Age":30}
```

### SerializeToBytes<T>(T value)
Serializes an object to a byte array.

```csharp
public byte[] SerializeToBytes<T>(T value)
```

**Type Parameters:**
- `T`: Type of object to serialize

**Parameters:**
- `value`: Object to serialize

**Returns:** `byte[]` - UTF-8 encoded JSON bytes

**Performance:** Uses buffer pooling for optimal memory usage

### SerializeToSpan<T>(T value, Span<byte> destination)
Serializes directly to a byte span for zero-copy operations.

```csharp
public int SerializeToSpan<T>(T value, Span<byte> destination)
```

**Type Parameters:**
- `T`: Type of object to serialize

**Parameters:**
- `value`: Object to serialize
- `destination`: Span to write JSON bytes into

**Returns:** `int` - Number of bytes written

**Example:**
```csharp
using System.Buffers;

var person = new Person { Name = "Bob" };
var buffer = ArrayPool<byte>.Shared.Rent(4096);
try {
    var bytesWritten = serializer.SerializeToSpan(person, buffer);
    var jsonSpan = buffer.AsSpan(0, bytesWritten);
    // Use jsonSpan directly
} finally {
    ArrayPool<byte>.Shared.Return(buffer);
}
```

### SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken)
Asynchronously serializes to a stream.

```csharp
public Task SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken = default)
```

**Parameters:**
- `value`: Object to serialize
- `stream`: Target stream
- `cancellationToken`: Optional cancellation token

**Example:**
```csharp
using var memoryStream = new MemoryStream();
await serializer.SerializeAsync(data, memoryStream, cancellationToken);
```

## Deserialization Methods

### Deserialize<T>(string json)
Deserializes JSON string to typed object.

```csharp
public T Deserialize<T>(string json)
```

**Type Parameters:**
- `T`: Target deserialization type

**Parameters:**
- `json`: JSON string to parse

**Returns:** `T` - Deserialized object

**Example:**
```csharp
string json = "{\"Name\":\"Alice\",\"Age\":30}";
Person person = serializer.Deserialize<Person>(json);
```

### Deserialize<T>(ReadOnlySpan<byte> json)
Deserializes from byte span for high performance.

```csharp
public T Deserialize<T>(ReadOnlySpan<byte> json)
```

**Type Parameters:**
- `T`: Target type

**Parameters:**
- `json`: JSON byte span

**Returns:** `T` - Deserialized object

### Deserialize(string json, Type type)
Deserializes to specified type dynamically.

```csharp
public object Deserialize(string json, Type type)
```

**Parameters:**
- `json`: JSON string
- `type`: Target type

**Returns:** `object` - Deserialized object instance

### DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken)
Asynchronously deserializes from stream.

```csharp
public Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
```

**Parameters:**
- `stream`: Source stream
- `cancellationToken`: Optional cancellation token

**Returns:** `Task<T?>` - Deserialized object

**Example:**
```csharp
using var fileStream = File.OpenRead("data.json");
Person data = await serializer.DeserializeAsync<Person>(fileStream);
```

## Type Support

### Supported Types

#### Primitive Types
- `bool`, `byte`, `sbyte`, `short`, `ushort`
- `int`, `uint`, `long`, `ulong`
- `float`, `double`, `decimal`
- `string`, `char`, `DateTime`, `DateTimeOffset`
- `Guid`

#### Collections
- Arrays: `T[]`, `IEnumerable<T>`
- Lists: `List<T>`, `IList<T>`, `ICollection<T>`
- Sets: `HashSet<T>`, `ISet<T>`
- Dictionaries: `Dictionary<TKey, TValue>`, `IDictionary<TKey, TValue>`

#### Complex Types
- Custom classes and structs
- Nullable types: `int?`, `DateTime?`, etc.
- Enums: string representation

#### Special Types
```csharp
// Automatic handling
var json = serializer.Serialize(Guid.NewGuid());              // "550e8400-e29b-41d4-a716-446655440000"
var guid = serializer.Deserialize<Guid>("\"550e8400...\"");

var dto = serializer.Deserialize<DateTimeOffset>("\"2023-09-03T21:19:28+03:00\"");
var ts = serializer.Deserialize<DateTime>("1693771168");       // Unix timestamp
```

## Configuration Integration

### Naming Strategies
```csharp
var config = new JsonConfiguration {
    NamingStrategy = NamingStrategy.CamelCase
};
var serializer = new FluxJsonSerializer(config);

class User { public string FirstName { get; set; } }
var json = serializer.Serialize(new User { FirstName = "John" });
// {"firstName":"John"} - CamelCase applied
```

### Null Handling
```csharp
var config = new JsonConfiguration {
    NullHandling = NullHandling.Ignore
};
var serializer = new FluxJsonSerializer(config);

class User {
    public string Name { get; set; }      // "John"
    public string Email { get; set; }     // null
}
var json = serializer.Serialize(new User { Name = "John", Email = null });
// {"Name":"John"} - null ignored
```

### DateTime Formats
```csharp
var config = new JsonConfiguration {
    DateTimeFormat = DateTimeFormat.ISO8601,
    CustomDateTimeFormat = "yyyy-MM-dd"
};
var serializer = new FluxJsonSerializer(config);

var date = DateTime.Now;
var json = serializer.Serialize(date);
// "2023-09-03T21:19:28.000Z" - ISO 8601
```

### Indented Output
```csharp
var config = new JsonConfiguration { WriteIndented = true };
var serializer = new FluxJsonSerializer(config);

var data = new { Name = "Test", Items = new[] { 1, 2, 3 } };
var json = serializer.Serialize(data);
// {
//   "Name": "Test",
//   "Items": [1, 2, 3]
// }
```

## Custom Converters

### Global Converters
```csharp
public class CustomConverter : IJsonConverter {
    public bool CanConvert(Type type) => type == typeof(CustomType);
    public void Write(ref JsonWriter writer, object value, JsonConfiguration config) { /* ... */ }
    public object Read(ref JsonReader reader, Type type, JsonConfiguration config) { /* ... */ }
}

// Register globally
var config = new JsonConfiguration();
config.Converters.Add(new CustomConverter());
var serializer = new FluxJsonSerializer(config);
```

### Type-Specific Converters
```csharp
var config = new JsonConfiguration();
config.TypeConverters[typeof(MyType)] = new MyCustomConverter();
var serializer = new FluxJsonSerializer(config);
```

### Attribute-Based Converters
```csharp
[JsonConverter(typeof(MyConverter))]
public class MyType {
    public string Value { get; set; }
}

// Automatic detection
var serializer = new FluxJsonSerializer();
var json = serializer.Serialize(new MyType());  // Uses MyConverter
```

## Performance Features

### Buffer Pooling
- Automatic buffer renting/returning for `SerializeToBytes`
- minimizes garbage collection pressure
- suitable for high-throughput scenarios

### Zero-Copy Operations
```csharp
// Direct to span - no allocations
Span<byte> buffer = stackalloc byte[1024];
var bytesWritten = serializer.SerializeToSpan(obj, buffer);
// Use buffer directly
```

### Memory-Efficient Streaming
```csharp
// For large objects or collections
using var stream = new FileStream("large.json", FileMode.Create);
await serializer.SerializeAsync(largeData, stream);
```

## Error Handling

### Exception Types
```csharp
try {
    var result = serializer.Deserialize<InvalidType>("invalid json");
} catch (JsonException ex) {
    // General JSON processing errors
    Console.WriteLine($"JSON Error: {ex.Message}");
} catch (JsonParseException ex) {
    // Parsing-specific errors
    Console.WriteLine($"Parse Error at {ex.Position}");
} catch (FormatException ex) {
    // Type conversion errors
    Console.WriteLine($"Format Error: {ex.Message}");
}
```

### Null Handling
```csharp
// Safe null handling
var result = serializer.Deserialize<ComplexType>("null");  // null
var result2 = serializer.Deserialize<int?>("null");       // null
var result3 = serializer.Deserialize<string>("null");     // null

// Safe empty strings
var result4 = serializer.Deserialize<ComplexType>("");    // default(T)
```

## Advanced Usage

### Mixed Content Streams
```csharp
public class ApiResponse<T> {
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}

var response = new ApiResponse<User> {
    Success = true,
    Message = "OK",
    Data = user
};
var json = serializer.Serialize(response);
```

### Collection Serialization
```csharp
// Arrays
int[] numbers = { 1, 2, 3 };
string json1 = serializer.Serialize(numbers);  // [1,2,3]

// Lists
List<Person> people = GetPeople();
string json2 = serializer.Serialize(people);

// Dictionaries
Dictionary<string, object> dict = new() {
    ["name"] = "Alice",
    ["age"] = 30,
    ["tags"] = new[] { "dev", "csharp" }
};
string json3 = serializer.Serialize(dict);
```

### Nested Objects
```csharp
public class Company {
    public string Name { get; set; }
    public List<Employee> Employees { get; set; }
}

public class Employee {
    public string Name { get; set; }
    public Department Department { get; set; }
}

// Automatic deep serialization
var company = new Company {
    Name = "TechCorp",
    Employees = new List<Employee> {
        new Employee { Name = "John", Department = new Department { Name = "Engineering" } }
    }
};
var json = serializer.Serialize(company);
```

## Best Practices

### Performance Optimization
```csharp
// 1. Reuse serializers for same configuration
var serializer = new FluxJsonSerializer(config);  // Create once
for(int i = 0; i < 1000; i++) {
    var json = serializer.Serialize(data);
}

// 2. Use byte spans for performance-critical code
Span<byte> buffer = stackalloc byte[4096];
var bytes = serializer.SerializeToSpan(obj, buffer);

// 3. Stream large data
await serializer.SerializeAsync(largeObj, networkStream);
```

### Error Handling
```csharp
// Validate inputs
public T SafeDeserialize<T>(string json) {
    if(string.IsNullOrWhiteSpace(json))
        return default;

    try {
        return serializer.Deserialize<T>(json);
    } catch(JsonException) {
        // Log and return default
        return default;
    }
}
```

### Configuration Reuse
```csharp
// Create configuration once
var config = new JsonConfiguration {
    NamingStrategy = NamingStrategy.CamelCase,
    WriteIndented = true,
    DateTimeFormat = DateTimeFormat.ISO8601
};

// Reuse across serializers
var serializer1 = new FluxJsonSerializer(config);
var serializer2 = new FluxJsonSerializer(config);
```

## Thread Safety

- Each `FluxJsonSerializer` instance is thread-safe
- Can be shared across multiple threads
- Configuration is immutable after creation
- Buffered operations use thread-safe buffer pooling

## Memory Management

### Automatic Cleanup
```csharp
// Buffer pools automatically managed
var bytes = serializer.SerializeToBytes(largeObj);
// No manual cleanup needed
```

### Manual Control
```csharp
// For fine-grained control
using var pooledBuffer = new ArrayPool<byte>.Shared.Rent(8192);
Span<byte> buffer = pooledBuffer;
// Use buffer with serializer
```

## See Also

- [Json Class](./Json.md) - Convenient static methods
- [JsonConfiguration](./JsonConfiguration.md) - Configuration options
- [IJsonConverter](./IJsonConverter.md) - Custom converter interface
- [JsonWriter](./JsonWriter.md) - Low-level writing
- [JsonReader](./JsonReader.md) - Low-level reading
