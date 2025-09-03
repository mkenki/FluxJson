# JsonSerializableAttribute 📝

The `JsonSerializableAttribute` is a marker attribute that enables source-generated serialization for classes and structs. When applied to a type, it signals the FluxJson source generator to create optimized serialization code at compile-time.

## Namespace
```csharp
using FluxJson.Core;
```

## Syntax
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class JsonSerializableAttribute : Attribute
{
    public JsonSerializableAttribute() { }
}
```

## Usage

### Basic Usage
```csharp
using FluxJson.Core;

[JsonSerializable]
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}

// Usage - automatically uses generated code
var product = new Product {
    Name = "Laptop",
    Price = 999.99m,
    Tags = new[] { "electronics", "gaming" }
};

string json = Json.Serialize(product);
// Uses source-generated methods automatically
```

### With IJsonSerializable Interface
```csharp
using FluxJson.Core;

[JsonSerializable]
public class User : IJsonSerializable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// The source generator implements the interface methods
User user = Json.Deserialize<User>(json);
```

## Benefits

### Performance Benefits
- **Zero Reflection**: Compile-time code generation eliminates runtime reflection
- **Optimized Paths**: Generated code is tailored specifically for your type
- **Better Performance**: Significantly faster than reflection-based serialization

### Development Benefits
- **Compile-time Checking**: Catch serialization errors at compile-time
- **IntelliSense Support**: IDE support for generated serialization methods
- **Type Safety**: Type-safe serialization without runtime exceptions

## Comparison

### Without [JsonSerializable] (Reflection-based)
```csharp
// Uses reflection at runtime
var json = Json.From(person).ToJson();
// Runtime: Introspect properties, create delegates, cache, etc.
```

### With [JsonSerializable] (Source-generated)
```csharp
// Uses compile-time generated code
[JsonSerializable]
public class Person { ... }

var json = Json.From(person).ToJson();
// Runtime: Direct property access, no reflection overhead
```

## Generated Code

The source generator creates methods that look similar to this:

```csharp
public partial class Product
{
    // Generated serialization method
    private string Serialize()
    {
        var builder = new StringBuilder();
        builder.Append("{\"Name\":");
        builder.Append(JsonSerializer.Serialize(Name));
        builder.Append(",\"Price\":");
        builder.Append(JsonSerializer.Serialize(Price));
        // ... optimized code for all properties
        return builder.ToString();
    }

    // Generated deserialization method
    private static Product Deserialize(string json)
    {
        // ... optimized parsing logic
    }
}
```

## Limitations

### Supported Types
- ✅ Classes and structs with public properties
- ✅ Primitive types (int, string, bool, etc.)
- ✅ Common collections (List<T>, Dictionary<TKey, TValue>)
- ✅ Nullable types
- ✅ Simple nested objects

### Not Supported (Yet)
- ❌ Circular references
- ❌ Complex inheritance hierarchies
- ❌ Dynamic properties
- ❌ Custom converters (limited support)

### Fallback Behavior
```csharp
// If source generation is not available, automatically falls back to reflection
[JsonSerializable]
public class ComplexType {
    // Complex logic that can't be source-generated
}

var result = Json.From(complexType).ToJson();
// Uses reflection-based serialization transparent to user
```

## Best Practices

### When to Use
```csharp
// ✅ High-performance scenarios
[JsonSerializable]
public class ApiResponse { ... }

// ✅ Frequently serialized types
[JsonSerializable]
public class Configuration { ... }

// ✅ Data transfer objects
[JsonSerializable]
public class Dto { ... }
```

### When to Avoid
```csharp
// ❌ Large, complex class hierarchies
[JsonSerializable]
public class MassiveEntity { ... }

// ❌ Types with dynamic properties
[JsonSerializable]
public class ExpandoObject { ... }

// ❌ Prototyping (use reflection first)
```

### Naming Convention
```csharp
[JsonSerializable]
public class UserProfile
{
    // Clear property names map well to JSON
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
```

## Integration with Configuration

Source-generated types work seamlessly with all FluxJson features:

```csharp
[JsonSerializable]
public class User
{
    public string Name { get; set; }
    public DateTime Created { get; set; }
}

// Configure with source-generated types
var json = Json.From(user)
    .Configure(config => config
        .UseNaming(NamingStrategy.CamelCase)
        .FormatDates(DateTimeFormat.ISO8601)
        .WriteIndented(true))
    .ToJson();
```

## Troubleshooting

### Common Issues

**Issue**: Source-generated code not being used
```csharp
// Solution: Ensure the FluxJson.Generator package is installed
// and the project file has the correct generator reference
```

**Issue**: "Method not found" errors
```csharp
// Solution: Rebuild the project to trigger code generation
// Clean -> Rebuild
```

**Issue**: Performance no different from reflection
```csharp
// Solution: Check that [JsonSerializable] is applied
// and FluxJson.Generator is properly configured
```

### Debugging
```csharp
// Enable detailed logging to see which serialization path is used
JsonConfiguration.Default.EnableDebugLogging = true;

var json = Json.Serialize(obj);
// Check logs to see if source-generated methods are being used
```

## See Also

- [Json Class](./Json.md) - Main entry point for serialization
- [FluxJsonGeneratedAttribute](./FluxJsonGeneratedAttribute.md) - Generated code marker
- [IJsonSerializable](./IJsonSerializable.md) - Interface for custom serialization
- [Source Generator Guide](../../guides/source-generators.md) - Detailed implementation guide
