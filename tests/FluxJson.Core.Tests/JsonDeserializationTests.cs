// tests/FluxJson.Core.Tests/JsonDeserializationTests.cs
using Xunit;
using FluentAssertions;
using FluxJson;
using FluxJson.Core.Tests;
using FluxJson.Core.Configuration;
using FluxJson.Core.Fluent;
using FluxJson.Core.Extensions;

namespace FluxJson.Core.Tests;

public class JsonDeserializationTests
{
    [Fact]
    public void Deserialize_ValidJson_ShouldReturnCorrectObject()
    {
        // Arrange
        var json = """{"Name":"John Doe","Age":30,"IsActive":true}""";

        // Act
        var person = Json.Parse(json).To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().Be("John Doe");
        person.Age.Should().Be(30);
        person.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_CamelCaseJson_ShouldMapToProperties()
    {
        // Arrange
        var json = """{"name":"John","age":30,"isActive":true}""";

        // Act
        var person = Json.Parse(json).UseCamelCase().To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().Be("John");
        person.Age.Should().Be(30);
        person.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_SnakeCaseJson_ShouldMapToProperties()
    {
        // Arrange
        var json = """{"first_name":"John","last_name":"Doe"}""";

        // Act
        var data = Json.Parse(json)
            .Configure(config => config.UseNaming(NamingStrategy.SnakeCase))
            .To<PersonData>();

        // Assert
        data.Should().NotBeNull();
        data!.FirstName.Should().Be("John");
        data!.LastName.Should().Be("Doe");
    }

    [Fact]
    public void Deserialize_WithNullValues_ShouldHandleNulls()
    {
        // Arrange
        var json = """{"Name":null,"Age":30,"IsActive":true}""";

        // Act
        var person = Json.Parse(json).To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().BeNull();
        person.Age.Should().Be(30);
        person.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_CaseInsensitive_ShouldMatchPropertiesIgnoringCase()
    {
        // Arrange
        var json = """{"name":"John","AGE":30,"isactive":true}""";

        // Act
        var person = Json.Parse(json).CaseInsensitive().To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().Be("John");
        person.Age.Should().Be(30);
        person.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_WithTrailingCommas_ShouldParseSuccessfully()
    {
        // Arrange
        var json = """{"Name":"John","Age":30,}""";

        // Act
        var person = Json.Parse(json).AllowTrailingCommas().To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().Be("John");
        person.Age.Should().Be(30);
    }

    [Fact]
    public void Deserialize_ComplexConfiguration_ShouldApplyAllSettings()
    {
        // Arrange
        var json = """{"name":"John","age":30,"isActive":true,}""";

        // Act
        var person = Json.Parse(json)
            .UseCamelCase()
            .AllowTrailingCommas()
            .CaseInsensitive()
            .Configure(config => config.WithPerformanceMode(PerformanceMode.Features))
            .To<Person>();

        // Assert
        person.Should().NotBeNull();
        person!.Name.Should().Be("John");
        person.Age.Should().Be(30);
        person.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("\"test string\"", "test string")]
    [InlineData("42", 42)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("null", null)]
    public void Deserialize_PrimitiveTypes_ShouldReturnCorrectValues(string json, object? expected)
    {
        // Act & Assert
        if (expected is string expectedStr)
        {
            var result = Json.Parse(json).To<string>();
            result.Should().Be(expectedStr);
        }
        else if (expected is int expectedInt)
        {
            var result = Json.Parse(json).To<int>();
            result.Should().Be(expectedInt);
        }
        else if (expected is bool expectedBool)
        {
            var result = Json.Parse(json).To<bool>();
            result.Should().Be(expectedBool);
        }
        else if (expected is null)
        {
            var result = Json.Parse(json).To<string>();
            result.Should().BeNull();
        }
    }
}
