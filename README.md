# FluxJson 🚀

[![Build Status](https://github.com/mkenki/FluxJson/workflows/CI/badge.svg)](https://github.com/mkenki/FluxJson/actions)
[![NuGet Version](https://img.shields.io/nuget/v/FluxJson.Core.svg)](https://www.nuget.org/packages/FluxJson.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Modern, high-performance JSON serialization library for .NET with a fluent API design, focusing on speed, flexibility, and developer experience.

## ✨ Features

- **🚀 High Performance**: Near-zero allocation using `Span<T>`, SIMD optimizations, and performance-optimized modes
- **🎯 Advanced Fluent API**: Intuitive, chainable API with comprehensive configuration options
- **⚡ Source Generators**: Compile-time code generation with `[JsonSerializable]` attribute for reflection-free serialization
- **🔧 Extensive Configuration**: Multiple naming strategies, flexible null handling, custom DateTime formats, and converter system
- **🔄 Custom Converters**: Built-in converters and support for custom type conversion logic
- **⚙️ Performance Modes**: Speed, Balanced, and Features modes to optimize for your specific use case
- **🧪 Comprehensive Testing**: Extensive unit tests, integration tests, and benchmark suites
- **📦 Modern .NET**: Built for .NET 8+ with latest C# features and runtime improvements

## 📦 Installation

### NuGet Package Manager
```bash
Install-Package FluxJson.Core
```

### .NET CLI
```bash
dotnet add package FluxJson.Core
```

### Package Reference
```xml
<PackageReference Include="FluxJson.Core" Version="1.0.0" />
```

> **Note**: Also install FluxJson.Generator package if you want compile-time source generation support.

## 🚀 Quick Start

### Basic Usage

```csharp
using FluxJson;

// Quick serialization and deserialization
var person = new { Name = "Alice", Age = 30 };

// Serialize to JSON
var json = Json.Serialize(person);
Console.WriteLine(json); // {"Name":"Alice","Age":30}

// Deserialize from JSON
var deserialized = Json.Deserialize<dynamic>(json);
Console.WriteLine($"Name: {deserialized.Name}, Age: {deserialized.Age}");
```

### Advanced Fluent API

```csharp
using FluxJson.Core.Configuration;

// Fluent serialization with comprehensive configuration
var user = new User
{
    FirstName = "Bob",
    LastName = "Smith",
    BirthDate = new DateTime(1990, 5, 15),
    Email = null,
    Settings = new UserSettings { Theme = "dark", Language = "en" }
};

var json = Json.From(user)
    .Configure(config => config
        .UseNaming(NamingStrategy.CamelCase)  // Convert to camelCase
        .HandleNulls(NullHandling.Ignore)     // Skip null values
        .FormatDates("yyyy-MM-dd")            // Custom date format
        .WriteIndented(true)                  // Pretty print JSON
        .WithPerformanceMode(PerformanceMode.Speed) // Optimize for speed
    )
    .ToJson();

Console.WriteLine(json);
// {
//   "firstName": "Bob",
//   "lastName": "Smith",
//   "birthDate": "1990-05-15",
//   "settings": {
//     "theme": "dark",
//     "language": "en"
//   }
// }
```

### Source Generator Support

```csharp
using FluxJson;

// Mark class for source-generated serialization (zero reflection)
[JsonSerializable]
public partial class Product : IJsonSerializable<Product>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string[] Tags { get; set; }
}

// Usage - automatically uses generated code
var product = new Product { Name = "Laptop", Price = 999.99m, Tags = new[] { "electronics", "gaming" } };
var json = Json.From(product).ToJson();
// Compile-time generated methods used automatically
```

### Custom Converters

```csharp
using FluxJson.Core.Converters;

// Register custom converter
JsonConfiguration.Default.Converters.Add(new CustomDateConverter());

public class CustomDateConverter : IJsonConverter
{
    // Implementation for custom date serialization/deserialization
}
```

## 📊 Performance

FluxJson is engineered to outperform existing .NET JSON serialization libraries. Benchmarks are continuously run to ensure optimal performance.

| Library | Serialize (ops/sec) | Deserialize (ops/sec) | Memory (KB) |
|---------|--------------------|--------------------|-------------|
| FluxJson | ⚡️ Excellent | ⚡️ Excellent | ⚡️ Minimal |
| System.Text.Json | Baseline | Baseline | Baseline |
| Newtonsoft.Json | ~50% slower | ~40% slower | ~2x more |

*Note: Detailed benchmark results will be provided as the project matures and stabilizes.*

## 🏗️ Architecture

```
FluxJson.Core/              # Core serialization library and public APIs
├── Configuration/          # Fluent configuration API and settings management
│   ├── Enums.cs           # NamingStrategy, NullHandling, PerformanceMode, etc.
│   └── JsonConfiguration.cs # Main configuration class
├── Converters/            # Built-in and custom type converters
│   ├── IJsonConverter.cs  # Converter interface
│   ├── JsonConverterAttribute.cs # Attribute for custom converters
│   └── BuiltInConverters.cs # Pre-built converters
├── Extensions/            # Extension methods for convenience
│   ├── StringExtensions.cs
│   └── JsonConfigurationExtensions.cs
├── Fluent/                # Advanced fluent API interfaces and implementations
│   ├── IJsonConfigurationBuilder.cs
│   ├── JsonConfigurationBuilder.cs
│   └── JsonBuilder.cs     # Fluent chaining support
├── Serialization/         # Low-level serialization/deserialization logic
│   ├── JsonBuilder.cs     # Core serialization engine
│   ├── DefaultJsonBuilder.cs
│   ├── ReflectionBasedJsonBuilder.cs
│   └── JsonSerializationLogic.cs
└── Global features       # Core types and utilities
    ├── IJsonSerializable.cs
    ├── JsonSerializableAttribute.cs
    └── TypeHelpers.cs

FluxJson.Generator/        # Source Generator for compile-time optimizations
├── JsonSourceGenerator.cs # Main generator with [JsonSerializable] support
├── JsonSerializationGenerator.cs
├── JsonDeserializationGenerator.cs
├── SyntaxReceiver.cs      # Roslyn syntax analysis
└── ...

FluxJson.Benchmarks/       # Performance testing suite using BenchmarkDotNet
└── Multiple benchmark classes for comprehensive performance analysis

tests/                     # Comprehensive test suite
├── FluxJson.Core.Tests/   # Unit tests for core functionality
├── FluxJson.Integration.Tests/ # Integration tests
└── ...
```

## ⚙️ Configuration Options

FluxJson offers extensive configuration options to fit any serialization scenario:

### Naming Strategies
- **NamingStrategy.CamelCase**: `FirstName` → `firstName`
- **NamingStrategy.SnakeCase**: `FirstName` → `first_name`
- **NamingStrategy.KebabCase**: `FirstName` → `first-name`
- **NamingStrategy.PascalCase**: `firstName` → `FirstName`

### Null Handling
- **NullHandling.Include**: Include null values in output
- **NullHandling.Ignore**: Skip null properties entirely

### DateTime Formatting
- **DateTimeFormat.ISO8601**: Standard ISO 8601 format
- **DateTimeFormat.UnixTimestamp**: Unix timestamp (seconds since epoch)
- **DateTimeFormat.Custom**: Use custom format string

### Performance Modes
- **PerformanceMode.Speed**: Maximum performance, minimal features
- **PerformanceMode.Balanced**: Balance between speed and features
- **PerformanceMode.Features**: Enable all features, slightly slower

### Advanced Options
- Custom converters for complex types
- JSON pretty-printing (WriteIndented)
- Trailing comma support
- Case-sensitive parsing
- Read-only property handling
- Maximum depth limits

## 🎯 Roadmap

### ✅ Completed Features
- [x] Project setup and core architecture
- [x] Initial core serialization engine
- [x] Advanced fluent API implementation with chaining
- [x] Source generator integration for basic types with `[JsonSerializable]` attribute
- [x] Multiple naming strategies (CamelCase, SnakeCase, KebabCase, PascalCase)
- [x] Flexible null handling (Include/Ignore)
- [x] Custom DateTime formatting (ISO8601, Unix timestamp, custom formats)
- [x] Performance modes (Speed, Balanced, Features)
- [x] Custom converter system with built-in converters
- [x] Extension methods for configuration and utilities
- [x] Comprehensive unit and integration test suite
- [x] BenchmarkDotNet performance testing suite
- [x] Token-based JSON parser with multiple token types
- [x] Attribute-based serialization control

### 🚧 In Progress / Planned
- [ ] Advanced performance optimizations (SIMD, Span<T> further integration)
- [ ] Comprehensive source generator support for complex types and configurations
- [ ] Detailed benchmark suite with public results and comparisons
- [ ] Extensive documentation and usage guides
- [ ] Support for custom attributes and advanced mapping scenarios
- [ ] JSON Schema validation and generation
- [ ] Streaming serialization for large datasets
- [ ] Plugin architecture for extensions
- [ ] Integration with popular .NET web frameworks

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

**Built with ❤️ for the .NET community.** 🚀
=======
**Built with ❤️ for the .
>>>>>>> 508d4b4f8125e2354fe49d0f0dc27b9a12c1b410
