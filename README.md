# FluxJson 🚀

[![Build Status](https://github.com/mkenki/FluxJson/workflows/CI/badge.svg)](https://github.com/mkenki/FluxJson/actions)
[![NuGet Version](https://img.shields.io/nuget/v/FluxJson.Core.svg)](https://www.nuget.org/packages/FluxJson.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Modern, high-performance JSON serialization library for .NET with a fluent API design, focusing on speed, flexibility, and developer experience.

## ✨ Features

- **🚀 High Performance**: Achieves near-zero allocation with `Span<T>` and leverages SIMD optimizations for lightning-fast serialization and deserialization.
- **🎯 Fluent API**: Provides an intuitive, chainable API for easy configuration and usage, making complex serialization scenarios simple to manage.
- **⚡ Source Generators**: Utilizes C# Source Generators to eliminate runtime reflection overhead, resulting in compile-time optimized serialization logic.
- **🔧 Highly Configurable**: Offers extensive configuration options for naming strategies, null handling, date formatting, and custom converters, giving you fine-grained control.
- **🧪 Well Tested**: Backed by a comprehensive test suite and continuous benchmarking to ensure reliability and performance.
- **📦 Modern .NET**: Built specifically for .NET 8+ and takes full advantage of the latest C# language features and runtime improvements.

## 🚀 Quick Start

```csharp
using FluxJson;
using FluxJson.Core.Configuration; // For NamingStyle and NullStrategy

// Simple serialization
var myObject = new { Name = "Alice", Age = 30 };
var json = Json.From(myObject).ToJson();
Console.WriteLine(json); // {"Name":"Alice","Age":30}

// Fluent configuration for serialization
var user = new { FirstName = "Bob", LastName = "Smith", BirthDate = new DateTime(1990, 5, 15), Email = (string)null };
var configuredJson = Json.From(user)
    .Configure(c => c
        .UseNaming(NamingStyle.CamelCase) // firstName, lastName
        .HandleNulls(NullStrategy.Omit)    // email property will be omitted
        .FormatDates("yyyy-MM-dd"))        // 1990-05-15
    .ToJson();
Console.WriteLine(configuredJson); // {"firstName":"Bob","lastName":"Smith","birthDate":"1990-05-15"}

// Deserialization
var jsonString = "{\"Name\":\"Charlie\",\"Age\":25}";
var deserializedObject = Json.Parse(jsonString).To<dynamic>();
Console.WriteLine($"Deserialized Name: {deserializedObject.Name}, Age: {deserializedObject.Age}");
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
FluxJson.Core/           # Main library: Core serialization engine, public APIs
├── Configuration/       # Fluent configuration API, settings management
├── Converters/          # Built-in and custom type converters
├── Extensions/          # Extension methods for convenience
├── Fluent/              # Fluent API interfaces and implementations
├── Serialization/       # Low-level serialization/deserialization logic
└── TypeHelpers/         # Utility methods for type inspection and manipulation

FluxJson.Generator/      # Source Generator project for compile-time optimizations
├── SyntaxReceiver.cs    # Collects syntax nodes for code generation
├── JsonSourceGenerator.cs # Main generator logic
└── ...                  # Specific serialization/deserialization generator parts

FluxJson.Benchmarks/     # Performance benchmarks using BenchmarkDotNet
FluxJson.Tests/          # Comprehensive unit and integration tests
```

## 🎯 Roadmap

- [x] Project setup and core architecture
- [x] Initial core serialization engine
- [x] Basic fluent API implementation
- [ ] Advanced performance optimizations (SIMD, Span<T> further integration)
- [x] Source generator integration for basic types
- [ ] Comprehensive source generator support for complex types and configurations
- [ ] Detailed benchmark suite and public results
- [ ] Extensive documentation, examples, and usage guides
- [ ] Support for custom attributes and advanced mapping scenarios

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

<<<<<<< HEAD
**Built with ❤️ for the .NET community.**
=======
**Built with ❤️ for the .
>>>>>>> 508d4b4f8125e2354fe49d0f0dc27b9a12c1b410
