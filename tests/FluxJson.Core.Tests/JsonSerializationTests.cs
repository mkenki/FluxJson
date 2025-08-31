// tests/FluxJson.Core.Tests/JsonSerializationTests.cs
using Xunit;
using FluentAssertions;
using FluxJson.Core.Tests;
using FluxJson.Core.Configuration;
using FluxJson.Core.Fluent;
using FluxJson.Core.Extensions;

namespace FluxJson.Core.Tests;

public class JsonSerializationTests
{
    [Fact]
    public void Serialize_SimpleObject_ShouldReturnValidJson()
    {
        // Arrange
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true
        };

        // Act
        var json = Json.From(person).ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"Name\":\"John Doe\"");
        json.Should().Contain("\"Age\":30");
        json.Should().Contain("\"IsActive\":true");
    }

    [Fact]
    public void Serialize_WithCamelCase_ShouldUseCamelCasePropertyNames()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var json = Json.From(person).UseCamelCase().ToJson();

        // Assert
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"age\":");
        json.Should().NotContain("\"Name\":");
        json.Should().NotContain("\"Age\":");
    }

    [Fact]
    public void Serialize_WithSnakeCase_ShouldUseSnakeCasePropertyNames()
    {
        // Arrange
        var data = new PersonData { FirstName = "John", LastName = "Doe" };

        // Act
        var json = Json.From(data).UseSnakeCase().ToJson();

        // Assert
        json.Should().Contain("\"first_name\":");
        json.Should().Contain("\"last_name\":");
    }

    [Fact]
    public void Serialize_WithNullValues_ShouldIncludeNullsByDefault()
    {
        // Arrange
        var person = new Person { Name = null, Age = 30 };

        // Act
        var json = Json.From(person).ToJson();

        // Assert
        json.Should().Contain("\"Name\":null");
    }

    [Fact]
    public void Serialize_IgnoreNulls_ShouldExcludeNullProperties()
    {
        // Arrange
        var person = new Person { Name = null, Age = 30, IsActive = true };

        // Act
        var json = Json.From(person).IgnoreNulls().ToJson();

        // Assert
        json.Should().NotContain("\"Name\":");
        json.Should().NotContain("null");
        json.Should().Contain("\"Age\":30");
        json.Should().Contain("\"IsActive\":true");
    }

    [Fact]
    public void Serialize_WriteIndented_ShouldFormatWithIndentation()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var json = Json.From(person).WriteIndented().ToJson();

        // Assert
        json.Should().Contain("{\n");
        json.Should().Contain("  ");
    }

    [Fact]
    public void Serialize_ComplexConfiguration_ShouldApplyAllSettings()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30, IsActive = true };

        // Act
        var json = Json.From(person)
            .UseCamelCase()
            .IgnoreNulls()
            .WriteIndented()
            .WithPerformanceMode(PerformanceMode.Features)
            .ToJson();

        // Assert
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"age\":");
        json.Should().Contain("\"isActive\":");
    }

    [Fact]
    public void SerializeToBytes_ShouldReturnValidByteArray()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var bytes = Json.From(person).ToBytes();

        // Assert
        bytes.Should().NotBeEmpty();
        var json = System.Text.Encoding.UTF8.GetString(bytes);
        json.Should().Contain("John");
    }

    [Fact]
    public void SerializeToSpan_ShouldWriteToProvidedSpan()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        Span<byte> buffer = stackalloc byte[1024];

        // Act
        var bytesWritten = Json.From(person).ToSpan(buffer);

        // Assert
        bytesWritten.Should().BeGreaterThan(0);
        var json = System.Text.Encoding.UTF8.GetString(buffer[..bytesWritten]);
        json.Should().Contain("John");
    }
}
