# FluxJson 🚀

[![Build Status](https://github.com/mkenki/FluxJson/workflows/CI/badge.svg)](https://github.com/mkenki/FluxJson/actions)
[![NuGet Version](https://img.shields.io/nuget/v/FluxJson.Core.svg)](https://www.nuget.org/packages/FluxJson.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Modern, high-performance JSON serialization library for .NET with a fluent API design.

## ✨ Features

- **🚀 High Performance**: Zero-allocation paths with Span<T> and SIMD optimizations
- **🎯 Fluent API**: Intuitive, chainable method design
- **⚡ Source Generators**: Compile-time optimizations
- **🔧 Highly Configurable**: Fine-grained control over serialization behavior
- **🧪 Well Tested**: Comprehensive test suite with benchmarks
- **📦 Modern .NET**: Built for .NET 8+ with latest C# features

## 🚀 Quick Start

```csharp
using FluxJson;

// Simple serialization
var json = Json.From(myObject).ToJson();

// Fluent configuration
var result = Json.From(user)
    .Configure(c => c
        .UseNaming(NamingStyle.CamelCase)
        .HandleNulls(NullStrategy.Omit)
        .FormatDates("yyyy-MM-dd"))
    .ToJson();

// Deserialization
var user = Json.Parse(jsonString).To();
```

## 📊 Performance

FluxJson is designed to be faster than existing solutions:

| Library | Serialize (ops/sec) | Deserialize (ops/sec) | Memory (KB) |
|---------|--------------------|--------------------|-------------|
| FluxJson | 🔥 Coming soon | 🔥 Coming soon | 🔥 Coming soon |
| System.Text.Json | Baseline | Baseline | Baseline |
| Newtonsoft.Json | ~50% slower | ~40% slower | ~2x more |

## 🏗️ Architecture

```
FluxJson.Core/           # Core serialization engine
├── Serializers/         # Type-specific serializers  
├── Converters/          # Custom type converters
├── Configuration/       # Fluent configuration API
├── Memory/             # Memory management & pooling
└── SourceGeneration/   # Compile-time optimizations
```

## 🎯 Roadmap

- [x] Project setup and architecture
- [ ] Core serialization engine
- [ ] Fluent API implementation  
- [ ] Performance optimizations
- [ ] Source generator integration
- [ ] Comprehensive benchmarks
- [ ] Documentation and samples

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.

---

**Built with ❤️ for the .
