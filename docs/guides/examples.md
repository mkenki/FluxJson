# Practical Examples 💡

This section demonstrates how to use FluxJson in various real-world scenarios.

## 🚀 Basic Examples

### Simple Serialization & Deserialization

```csharp
using FluxJson;

// Simple types
var person = new { Name = "John", Age = 30 };
string json = Json.Serialize(person);
// {"Name":"John","Age":30}

Person obj = Json.Deserialize<Person>(json);
```

### Collections & Arrays

```csharp
// Arrays
string[] names = { "Alice", "Bob", "Charlie" };
string json = Json.Serialize(names);
// ["Alice","Bob","Charlie"]

// Lists
List<int> numbers = new() { 1, 2, 3, 4, 5 };
string json = Json.Serialize(numbers);
// [1,2,3,4,5]

// Custom objects
var users = new List<User> {
    new User { Name = "John", Email = "john@test.com" },
    new User { Name = "Jane", Email = "jane@test.com" }
};
string json = Json.Serialize(users);
```

### Dictionaries

```csharp
var settings = new Dictionary<string, object> {
    ["theme"] = "dark",
    ["language"] = "en",
    ["notifications"] = true
};
string json = Json.Serialize(settings);
// {"theme":"dark","language":"en","notifications":true}
```

### Custom Classes

```csharp
public class Address {
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

public class Customer {
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HomeAddress { get; set; }
    public List<string> PhoneNumbers { get; set; }
}

var customer = new Customer {
    Id = 1,
    Name = "Alice Smith",
    HomeAddress = new Address {
        Street = "123 Main St",
        City = "New York",
        Country = "USA"
    },
    PhoneNumbers = new List<string> { "555-0123", "555-0456" }
};

string json = Json.Serialize(customer, indented: true);
// {
//   "Id": 1,
//   "Name": "Alice Smith",
//   "HomeAddress": {
//     "Street": "123 Main St",
//     "City": "New York",
//     "Country": "USA"
//   },
//   "PhoneNumbers": ["555-0123","555-0456"]
// }
```

## 🎨 Configuration Examples

### Naming Strategies

```csharp
var user = new { FirstName = "John", LastName = "Doe", UserId = 123 };

// CamelCase for JavaScript APIs
var json1 = Json.From(user)
    .Configure(c => c.UseNaming(NamingStrategy.CamelCase))
    .ToJson();
// {"firstName":"John","lastName":"Doe","userId":123}

// SnakeCase for Python APIs
var json2 = Json.From(user)
    .Configure(c => c.UseNaming(NamingStrategy.SnakeCase))
    .ToJson();
// {"first_name":"John","last_name":"Doe","user_id":123}
```

### Pretty Printing

```csharp
var product = new { Name = "Laptop", Price = 999, InStock = true };

// Compact (default)
string compact = Json.Serialize(product);
// {"Name":"Laptop","Price":999,"InStock":true}

// Pretty printed
var pretty = Json.From(product)
    .Configure(c => c.WriteIndented(true))
    .ToJson();
// {
//   "Name": "Laptop",
//   "Price": 999,
//   "InStock": true
// }
```

### Null Handling

```csharp
var data = new { Name = "John", Email = (string)null, Age = 25 };

// Include nulls (default)
string withNulls = Json.Serialize(data);
// {"Name":"John","Email":null,"Age":25}

// Ignore nulls (cleaner output)
var withoutNulls = Json.From(data)
    .Configure(c => c.HandleNulls(NullHandling.Ignore))
    .ToJson();
// {"Name":"John","Age":25}
```

### DateTime Formatting

```csharp
var event = new { Title = "Meeting", Time = DateTime.Now };

// ISO 8601 format
var iso = Json.From(event)
    .Configure(c => c.FormatDates(DateTimeFormat.ISO8601))
    .ToJson();
// {"Title":"Meeting","Time":"2023-09-03T21:27:53.000Z"}

// Unix timestamp
var unix = Json.From(event)
    .Configure(c => c.FormatDates(DateTimeFormat.UnixTimestamp))
    .ToJson();
// {"Title":"Meeting","Time":1693774073}
```

## 🔧 Advanced Examples

### Custom Converters

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

// Register globally
var config = new JsonConfiguration();
config.Converters.Add(new GuidConverter());
var serializer = new FluxJsonSerializer(config);

var data = new { Id = Guid.NewGuid(), Name = "Test" };
string json = serializer.Serialize(data);
// {"Id":"550e8400e29b41d4a716446655440000","Name":"Test"}
```

### Source Generation

```csharp
using FluxJson.Core;

[JsonSerializable]
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}

// Usage - automatically uses source-generated code for better performance
var product = new Product {
    Id = 1,
    Name = "Wireless Headphones",
    Price = 299.99m,
    Tags = new[] { "audio", "wireless", "bluetooth" }
};
string json = Json.Serialize(product);
```

## 🌐 Web API Integration

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase {
    private readonly FluxJsonSerializer _serializer;

    public ProductsController(FluxJsonSerializer serializer) {
        _serializer = serializer; // Injected via DI
    }

    [HttpGet]
    public IActionResult GetProducts() {
        var products = _productService.GetAllProducts();
        return Ok(products); // ASP.NET Core automatically uses configured serializer
    }

    [HttpPost]
    public IActionResult CreateProduct([FromBody] Product product) {
        var created = _productService.CreateProduct(product);
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request) {
        // Validate input
        if (!ModelState.IsValid) {
            return BadRequest(new ApiResponse<object> {
                Success = false,
                Message = "Validation failed",
                Errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
            });
        }

        var updated = await _productService.UpdateProductAsync(id, request);
        return Ok(new ApiResponse<Product> {
            Success = true,
            Message = "Product updated successfully",
            Data = updated
        });
    }
}
```

### Consistent API Response Format

```csharp
public class ApiResponse<T> {
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public Dictionary<string, string[]> Errors { get; set; }
}
```

### Dependency Injection Setup

```csharp
// Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services) {
    // Configure FluxJson serializer for the entire application
    services.AddSingleton<FluxJsonSerializer>(provider => {
        var config = new JsonConfiguration {
            NamingStrategy = NamingStrategy.CamelCase,
            WriteIndented = false,
            NullHandling = NullHandling.Ignore,
            DateTimeFormat = DateTimeFormat.ISO8601,
            PerformanceMode = PerformanceMode.Balanced
        };
        return new FluxJsonSerializer(config);
    });
}
```

## ⚡ Performance Examples

### High-Performance Serialization

```csharp
// For maximum performance in high-throughput scenarios
var config = new JsonConfiguration {
    PerformanceMode = PerformanceMode.Speed,
    WriteIndented = false,
    IgnoreReadOnlyProperties = true
};
var fastSerializer = new FluxJsonSerializer(config);

// Reuse the same serializer instance
for(int i = 0; i < 10000; i++) {
    var json = fastSerializer.Serialize(myData);
}
```

### Memory-Efficient Large Objects

```csharp
// Using buffer pooling for large objects
var serializer = new FluxJsonSerializer();

void SerializeLargeObject(T largeObj) {
    var buffer = ArrayPool<byte>.Shared.Rent(4096);
    try {
        var bytesWritten = serializer.SerializeToSpan(largeObj, buffer);
        var jsonSpan = buffer.AsSpan(0, bytesWritten);
        // Use jsonSpan directly without additional allocations
    } finally {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
```

### Streaming for Very Large Data

```csharp
public async Task SerializeToFileAsync<T>(T data, string filePath) {
    var serializer = new FluxJsonSerializer();
    using var fileStream = File.Create(filePath);
    await serializer.SerializeAsync(data, fileStream);
}

public async Task<T> DeserializeFromFileAsync<T>(string filePath) {
    var serializer = new FluxJsonSerializer();
    using var fileStream = File.OpenRead(filePath);
    return await serializer.DeserializeAsync<T>(fileStream);
}
```

## 🔍 Error Handling Examples

### Comprehensive Error Handling

```csharp
public class SafeSerializer {
    private readonly FluxJsonSerializer _serializer;

    public SafeSerializer() {
        _serializer = new FluxJsonSerializer();
    }

    public T? SafeDeserialize<T>(string json) {
        try {
            return _serializer.Deserialize<T>(json);
        } catch (JsonParseException ex) {
            _logger.LogError($"JSON parse error at position {ex.Position}: {ex.Message}");
            return default;
        } catch (JsonException ex) {
            _logger.LogError($"JSON processing error: {ex.Message}");
            return default;
        } catch (Exception ex) {
            _logger.LogError($"Unexpected error: {ex.Message}");
            return default;
        }
    }

    public (T? Data, string? Error) TryDeserialize<T>(string json) {
        try {
            var data = _serializer.Deserialize<T>(json);
            return (data, null);
        } catch (Exception ex) {
            return (default, ex.Message);
        }
    }
}

// Usage
var safeSerializer = new SafeSerializer();
var result = safeSerializer.TryDeserialize<User>(jsonString);

if (result.Data != null) {
    // Use result.Data
} else {
    Console.WriteLine($"Failed to deserialize: {result.Error}");
}
```

## 🎯 Common Patterns

### Helper Classes

```csharp
public static class JsonHelper {
    private static readonly Lazy<FluxJsonSerializer> _defaultSerializer = new(() =>
        new FluxJsonSerializer(new JsonConfiguration {
            NamingStrategy = NamingStrategy.CamelCase,
            WriteIndented = true,
            DateTimeFormat = DateTimeFormat.ISO8601
        }));

    private static readonly Lazy<FluxJsonSerializer> _compactSerializer = new(() =>
        new FluxJsonSerializer(new JsonConfiguration {
            WriteIndented = false,
            NamingStrategy = NamingStrategy.CamelCase
        }));

    public static string SerializePretty<T>(T obj) =>
        _defaultSerializer.Value.Serialize(obj);

    public static string SerializeCompact<T>(T obj) =>
        _compactSerializer.Value.Serialize(obj);

    public static T Deserialize<T>(string json) =>
        _defaultSerializer.Value.Deserialize<T>(json);

    public static bool TryDeserialize<T>(string json, out T result, out string error) {
        try {
            result = _defaultSerializer.Value.Deserialize<T>(json)!;
            error = null!;
            return true;
        } catch (Exception ex) {
            result = default!;
            error = ex.Message;
            return false;
        }
    }
}
```

### Template-Based Serialization

```csharp
public class JsonTemplate<T> {
    private readonly string _template;
    private readonly FluxJsonSerializer _serializer;

    public JsonTemplate(string template) {
        _template = template;
        _serializer = new FluxJsonSerializer();
    }

    public string Render(T model) {
        var serialized = _serializer.Serialize(model);
        return _template.Replace("{{data}}", serialized);
    }
}

// Usage with templates
var template = @"
{
  ""timestamp"": ""{{timestamp}}"",
  ""level"": ""info"",
  ""data"": {{data}}
}";

var logTemplate = new JsonTemplate<LogData>(template);
var logEntry = new LogData { Message = "User logged in", UserId = 123 };
string json = logTemplate.Render(logEntry);
```

## 📊 Complete Application Example

```csharp
public class EcommerceApi {
    private readonly FluxJsonSerializer _serializer;
    private readonly IProductRepository _products;

    public EcommerceApi(IProductRepository products) {
        var config = new JsonConfiguration {
            NamingStrategy = NamingStrategy.CamelCase,
            DateTimeFormat = DateTimeFormat.ISO8601,
            NullHandling = NullHandling.Ignore
        };
        _serializer = new FluxJsonSerializer(config);
        _products = products;
    }

    public async Task<ApiResponse<List<Product>>> GetProductsAsync(GetProductsRequest request) {
        var products = await _products.GetProductsAsync(request.Category, request.Page, request.PageSize);

        return new ApiResponse<List<Product>> {
            Success = true,
            Message = $"Found {products.Count} products",
            Data = products,
            Timestamp = DateTime.Now
        };
    }

    public async Task<ApiResponse<Product>> CreateProductAsync(CreateProductRequest request) {
        if (!ValidateRequest(request)) {
            return new ApiResponse<Product> {
                Success = false,
                Message = "Invalid product data",
                Errors = new Dictionary<string, string[]> {
                    ["name"] = new[] { "Product name is required" },
                    ["price"] = new[] { "Price must be greater than 0" }
                }
            };
        }

        var product = new Product {
            Id = GenerateId(),
            Name = request.Name,
            Price = request.Price,
            Category = request.Category,
            Created = DateTime.Now
        };

        await _products.SaveAsync(product);

        return new ApiResponse<Product> {
            Success = true,
            Message = "Product created successfully",
            Data = product
        };
    }

    public async Task<ApiResponse<BulkOperationResult>> BulkUpdateAsync(List<UpdateProductRequest> requests) {
        var results = new List<BulkOperationResult>();
        var successCount = 0;

        foreach (var request in requests) {
            try {
                await _products.UpdateAsync(request.Id, request);
                successCount++;
            } catch (Exception ex) {
                results.Add(new BulkOperationResult {
                    Id = request.Id,
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        return new ApiResponse<BulkOperationResult> {
            Success = true,
            Message = $"Bulk update completed: {successCount}/{requests.Count} successful",
            Data = new BulkOperationResult {
                TotalCount = requests.Count,
                SuccessCount = successCount,
                Results = results
            }
        };
    }
}

public class GetProductsRequest {
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class CreateProductRequest {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

public class UpdateProductRequest {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class BulkOperationResult {
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public List<BulkOperationResult>? Results { get; set; }
}

public class BulkOperationResult {
    public int Id { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
```

This comprehensive example shows real-world usage patterns including:
- Consistent API response formats
- Input validation
- Error handling
- Bulk operations
- Proper dependency injection setup
- Template usage for structured data

## 📚 Learn More

- [API Reference](./api.md) - Complete API documentation
- [Configuration Guide](./configuration.md) - All configuration options
- [Migration Guide](./migration.md) - Coming from other JSON libraries

See Also:
- [Getting Started](./getting-started.md)
- [Architecture Overview](./architecture.md)
