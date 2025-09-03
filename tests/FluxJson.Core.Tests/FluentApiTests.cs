// tests/FluxJson.Core.Tests/FluentApiTests.cs
using FluentAssertions;
using FluxJson.Core.Configuration;
using FluxJson.Core.Extensions; // Add this using directive
using FluxJson.Core.Tests;
using Xunit;

namespace FluxJson.Core.Tests;

public class FluentApiTests
{
    [Fact]
    public void FluentApi_ShouldChainMethodsCalls()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var json = Json.From(person)
            .UseCamelCase()
            .IgnoreNulls()
            .WriteIndented()
            .WithPerformanceMode(PerformanceMode.Speed)
            .ToJson();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"age\":");
    }

    [Fact]
    public void FluentApi_Configure_ShouldApplyCustomConfiguration()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var json = Json.From(person)
            .Configure(config => config
                .UseNaming(NamingStrategy.SnakeCase)
                .HandleNulls(NullHandling.Ignore)
                .WriteIndented(true))
            .ToJson();

        // Assert
        json.Should().Contain("\"age\":");
        // Should contain snake_case if there were multi-word properties
    }

    [Fact]
    public void QuickMethods_ShouldWorkWithoutFluentApi()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };

        // Act
        var json = Json.Serialize(person);
        var deserialized = Json.Deserialize<Person>(json);

        // Assert
        json.Should().NotBeNullOrEmpty();
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("John");
        deserialized.Age.Should().Be(30);
    }
}
