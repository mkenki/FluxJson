# Configuration Guide ⚙️

This comprehensive guide covers all configuration options available in FluxJson, including naming strategies, null handling, DateTime formatting, performance modes, and advanced settings.

## Introduction

FluxJson provides extensive configuration options through the `JsonConfiguration` class. You can configure serialization behavior globally or per-operation using the fluent API.

## Basic Configuration

### Creating Configuration
```csharp
using FluxJson.Core.Configuration;

// Method 1: Direct instantiation
var config = new JsonConfiguration
{
    NamingStrategy = NamingStrategy.CamelCase,
    WriteIndented = true,
    IgnoreReadOnlyProperties = false
};

// Method 2: Using fluent API
var config = JsonConfiguration.Default
    .WithNaming(NamingStrategy.CamelCase)
    .WithIndented(true)
    .IgnoreReadOnlyProperties(false);
```

### Using with Serializer
```csharp
// With class-based serializer
var serializer = new FluxJsonSerializer(config);

// With fluent API
var json = Json.From(obj)
    .Configure(config => config
        .UseNaming(NamingStrategy.CamelCase)
        .HandleNulls(NullHandling.Ignore))
    .ToJson();
```

## Naming Strategies

### Available Strategies

| Strategy | Example Input | Example Output |
|----------|---------------|----------------|
| **Default** | `FirstName` | `FirstName` |
| **CamelCase** | `FirstName` | `firstName` |
| **PascalCase** | `firstName` | `FirstName` |
| **SnakeCase** | `FirstName` | `first_name` |
| **KebabCase** | `FirstName` | `first-name` |

### Usage Examples
```csharp
class UserData {
    public string FirstName { get; set; }      // "John"
    public string LastName { get; set; }       // "Doe"
    public string UserId { get; set; }         // "12345"
    public bool IsActive { get; set; }         // true
}

var data = new UserData { FirstName = "John", LastName = "Doe" };
```

#### CamelCase Output
```csharp
var config = new JsonConfiguration { NamingStrategy = NamingStrategy.CamelCase };
var result = new FluxJsonSerializer(config).Serialize(data);
// {"firstName":"John","lastName":"Doe","userId":"12345","isActive":true}
```

#### SnakeCase Output
```csharp
var config = new JsonConfiguration { NamingStrategy = NamingStrategy.SnakeCase };
var result = new FluxJsonSerializer(config).Serialize(data);
// {"first_name":"John","last_name":"Doe","user_id":"12345","is_active":true}
```

#### KebabCase Output
```csharp
var config = new JsonConfiguration { NamingStrategy = NamingStrategy.KebabCase };
var result = new FluxJsonSerializer(config).Serialize(data);
// {"first-name":"John","last-name":"Doe","user-id":"12345","is-active":true}
```

### Fluent API Usage
```csharp
var json = Json.From(data)
    .Configure(c => c.UseNaming(NamingStrategy.CamelCase))
    .ToJson();

var json = Json.From(data)
    .Configure(c => c.UseNaming(NamingStrategy.SnakeCase))
    .ToJson();
```

## Null Handling

### Null Handling Options

| Option | Behavior | Example |
|--------|----------|---------|
| **Include** | Include null values | `{"name":"John","email":null}` |
| **Ignore** | Skip null properties | `{"name":"John"}` |
| **Default** | Same as Include | `{"name":"John","email":null}` |

### Detailed Examples

```csharp
class UserProfile {
    public string Name { get; set; }           // "Alice"
    public string Email { get; set; }          // null
    public string Phone { get; set; }          // null
    public int? Age { get; set; }              // null
    public string Address { get; set; }        // "123 Main St"
}

var profile = new UserProfile {
    Name = "Alice",
    Email = null,
    Phone = null,
    Age = null,
    Address = "123 Main St"
};
```

#### Include Nulls (Default)
```csharp
var config = new JsonConfiguration {
    NullHandling = NullHandling.Include
};
var result = new FluxJsonSerializer(config).Serialize(profile);
// {"name":"Alice","email":null,"phone":null,"age":null,"address":"123 Main St"}
```

## Dates and Times

### DateTime Format Options

| Format | Example Output | Description |
|--------|----------------|-------------|
| **Default** | `\/Date(1693771168000)\/` | .NET default |
| **ISO8601** | `2023-09-03T21:19:28.000Z` | Standard ISO format |
| **UnixTimestamp** | `1693771168` | Seconds since epoch |
| **Custom** | Uses custom format string | Flexible formatting |

### Usage Examples

```csharp
class Event {
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
}

var @event = new Event {
    Title = "Tech Conference",
    StartDate = new DateTime(2023, 9, 3, 9, 0, 0),
    EndDate = new DateTime(2023, 9, 3, 17, 0, 0),
    ScheduledAt = DateTimeOffset.Now
};
```

#### ISO 8601 Format
```csharp
var config = new JsonConfiguration {
    DateTimeFormat = DateTimeFormat.ISO8601
};
var result = new FluxJsonSerializer(config).Serialize(@event);
// {
//   "title": "Tech Conference",
//   "startDate": "2023-09-03T09:00:00.000Z",
//   "endDate": "2023-09-03T17:00:00.000Z",
//   "scheduledAt": "2023-09-03T21:19:28.000+03:00"
// }
```

#### Unix Timestamp
```csharp
var config = new JsonConfiguration {
    DateTimeFormat = DateTimeFormat.UnixTimestamp
};
var result = new FluxJsonSerializer(config).Serialize(@event);
// {
//   "title": "Tech Conference",
//   "startDate": 1693723200,
//   "endDate": 1693752000,
//   "scheduledAt": 1693771168
// }
```

#### Custom Format
```csharp
var config = new JsonConfiguration {
    DateTimeFormat = DateTimeFormat.Custom,
    CustomDateTimeFormat = "yyyy-MM-dd HH:mm:ss"
};
var result = new FluxJsonSerializer(config).Serialize(@event);
// {
//   "title": "Tech Conference",
//   "startDate": "2023-09-03 09:00:00",
//   "endDate": "2023-09-03 17:00:00",
//   "scheduledAt": "2023-09-03 21:19:28"
// }
```

## Performance Modes

### Performance Mode Options

| Mode | Speed | Memory | Features |
|------|-------|--------|----------|
| **Speed** | ⚡ Highest | 🔸 Lowest | Limited |
| **Balanced** | ⚡ Medium | 🔸 Medium | Most features |
| **Features** | 🐌 Lowest | 🧲 Highest | All features |

### Configuring Performance Mode

```csharp
// Speed mode for maximum performance
var speedConfig = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Speed
};
var fastSerializer = new FluxJsonSerializer(speedConfig);

// Balanced mode (recommended for most use cases)
var balancedConfig = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Balanced
};
var balancedSerializer = new FluxJsonSerializer(balancedConfig);

// Features mode for all capabilities
var featuresConfig = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Features
};
var featureSerializer = new FluxJsonSerializer(featuresConfig);
```

### Performance Mode Characteristics

#### Speed Mode Limitations
```csharp
// ❌ Custom converters may not work
// ❌ Advanced validation disabled
// ❌ Limited error messages
// ❌ Basic null handling only

var config = new JsonConfiguration { PerformanceMode = PerformanceMode.Speed };
```

#### Features Mode Benefits
```csharp
// ✅ Full custom converter support
// ✅ Comprehensive validation
// ✅ Detailed error reporting
// ✅ All advanced features available

var config = new JsonConfiguration { PerformanceMode = PerformanceMode.Features };
```

## Advanced Options

### Indented Output
```csharp
var config = new JsonConfiguration { WriteIndented = true };
var serializer = new FluxJsonSerializer(config);

class Person { public string Name { get; set; } public int Age { get; set; } }
var person = new Person { Name = "John", Age = 30 };

var json = serializer.Serialize(person);
// {
//   "name": "John",
//   "age": 30
// }
```

### Case Sensitivity
```csharp
var config = new JsonConfiguration { CaseSensitive = false };
var serializer = new FluxJsonSerializer(config);

// JSON properties can match C# properties regardless of case
string json = "{\"firstname\":\"John\",\"lastname\":\"Doe\",\"AGE\":25}";
var person = serializer.Deserialize<Person>(json); // Works even with mixed case
```

### Read-Only Properties
```csharp
class UserStats {
    public string Username { get; set; }
    public int LoginCount { get; set; }
    public DateTime LastLogin { get; set; }

    // Read-only property
    public bool IsActive => LoginCount > 0;
}

var config = new JsonConfiguration {
    IgnoreReadOnlyProperties = true  // Skip read-only properties during serialization
};
var serializer = new FluxJsonSerializer(config);

var stats = new UserStats { Username = "john", LoginCount = 5, LastLogin = DateTime.Now };
var json = serializer.Serialize(stats);
// {"username":"john","loginCount":5,"lastLogin":"2023-09-03T21:19:28.000Z"}
// "isActive" property is ignored because it's read-only
```

### Trailing Commas
```csharp
var config = new JsonConfiguration {
    AllowTrailingCommas = true
};
var serializer = new FluxJsonSerializer(config);

// This would normally fail, but with trailing comma support it works
string json = "{\"name\":\"John\",\"age\":30,}"; // Note the trailing comma
var person = serializer.Deserialize<Person>(json);
```

## Custom Converters

### Global Converters
```csharp
public class CustomGuidConverter : IJsonConverter {
    public bool CanConvert(Type type) => type == typeof(Guid);

    public void Write(ref JsonWriter writer, object value, JsonConfiguration config) {
        var guid = (Guid)value;
        writer.WriteValue(guid.ToString("N")); // Compact format without hyphens
    }

    public object Read(ref JsonReader reader, Type type, JsonConfiguration config) {
        var guidString = reader.ReadString();
        return Guid.TryParse(guidString, out var result) ? result : Guid.Empty;
    }
}

// Register globally
var config = new JsonConfiguration();
config.Converters.Add(new CustomGuidConverter());
var serializer = new FluxJsonSerializer(config);
```

### Type-Specific Converters
```csharp
var config = new JsonConfiguration();
config.TypeConverters[typeof(Person)] = new PersonConverter();
config.PropertyConverters[typeof(Company)][nameof(Company.PrimaryContact)] = new ContactConverter();

var serializer = new FluxJsonSerializer(config);
```

### Fluent API Converters
```csharp
var json = Json.From(company)
    .Configure(c => c
        .RegisterConverter(new CustomConverter())
        .RegisterConverterForType(typeof(Person), new PersonConverter())
        .RegisterConverterForProperty(typeof(Company), "PrimaryContact", new ContactConverter()))
    .ToJson();
```

## Configuration Best Practices

### Performance Tuning
```csharp
// For high-throughput APIs
var config = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Speed,
    WriteIndented = false,
    IgnoreReadOnlyProperties = true,
    NullHandling = NullHandling.Ignore
};

// For development/debugging
var debugConfig = new JsonConfiguration {
    WriteIndented = true,
    PerformanceMode = PerformanceMode.Features,
    AllowTrailingCommas = true
};

// For data exchange with external systems
var exchangeConfig = new JsonConfiguration {
    NamingStrategy = NamingStrategy.SnakeCase,
    DateTimeFormat = DateTimeFormat.ISO8601,
    CaseSensitive = false
};
```

### Environment-Based Configuration
```csharp
JsonConfiguration CreateConfiguration() {
    #if DEBUG
        return new JsonConfiguration {
            WriteIndented = true,
            PerformanceMode = PerformanceMode.Features
        };
    #else
        return new JsonConfiguration {
            WriteIndented = false,
            PerformanceMode = PerformanceMode.Balanced
        };
    #endif
}
```

### Configuration Sharing
```csharp
// Create base configuration
var baseConfig = new JsonConfiguration {
    DateTimeFormat = DateTimeFormat.ISO8601,
    NullHandling = NullHandling.Ignore
};

// Extend for specific use cases
var apiConfig = baseConfig.WithNaming(NamingStrategy.CamelCase);
var storageConfig = baseConfig.WithNaming(NamingStrategy.SnakeCase).WithIndented(true);

// Use different configurations for different contexts
var apiSerializer = new FluxJsonSerializer(apiConfig);
var storageSerializer = new FluxJsonSerializer(storageConfig);
```

## Configuration Validation

### Checking Configuration
```csharp
var config = new JsonConfiguration {
    NamingStrategy = NamingStrategy.CamelCase,
    PerformanceMode = PerformanceMode.Speed
};

// Validate configuration
if (!IsValidConfiguration(config)) {
    throw new InvalidOperationException("Invalid configuration detected");
}

bool IsValidConfiguration(JsonConfiguration config) {
    // Speed mode + WriteIndented = suboptimal
    if (config.PerformanceMode == PerformanceMode.Speed && config.WriteIndented) {
        return false;
    }

    // Custom format requires format string
    if (config.DateTimeFormat == DateTimeFormat.Custom &&
        string.IsNullOrEmpty(config.CustomDateTimeFormat)) {
        return false;
    }

    return true;
}
```

## Troubleshooting

### Common Configuration Issues

**Issue**: Configuration not applied
```csharp
// Problem: Using new serializer instance
var config = new JsonConfiguration { WriteIndented = true };
var result = Json.Serialize(obj); // Uses default config, not custom

// Solution: Use FluxJsonSerializer
var serializer = new FluxJsonSerializer(config);
var result = serializer.Serialize(obj);
```

**Issue**: Fluent API configuration ignored
```csharp
// Problem: Chain methods correctly
Json.From(obj).Configure(c => c.UseNaming(NamingStrategy.CamelCase)).ToJson();

// Common mistake: forgetting Configure callback
// Json.From(obj).UseNaming(NamingStrategy.CamelCase).ToJson(); // ❌ Wrong
// Json.From(obj).Configure(c => c.UseNaming(NamingStrategy.CamelCase)).ToJson(); // ✅ Correct
```

**Issue**: Custom converters not working
```csharp
// Problem: Registration order matters
var config = new JsonConfiguration();
config.Converters.Add(new MyConverter()); // Add before creating serializer
var serializer = new FluxJsonSerializer(config);
```

**Issue**: Performance different than expected
```csharp
// Check performance mode interactions
var config = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Speed,
    WriteIndented = false, // Indented output slows down even in Speed mode
    NullHandling = NullHandling.Ignore // This can help performance
};
```

## Next Steps

Now that you understand configuration options:

- [Getting Started Guide](./getting-started.md) - Learn basic usage
- [API Reference](../api/README.md) - Complete API documentation
- [Examples](../examples/README.md) - Practical usage examples
- [Migration Guide](./migration.md) - Switch from other JSON libraries

## Summary

FluxJson configuration is powerful and flexible:

- **Naming Strategies**: Transform property names (CamelCase, SnakeCase, etc.)
- **Null Handling**: Include or ignore null values
- **DateTime Formatting**: Control how dates are serialized
- **Performance Modes**: Balance speed vs features
- **Custom Converters**: Handle complex types
- **Advanced Options**: Fine-tune behavior

Choose configurations based on your specific use case, performance requirements, and interoperability needs.
