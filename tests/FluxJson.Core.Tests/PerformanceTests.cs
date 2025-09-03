using FluentAssertions;
using FluxJson.Core.Tests;
using Xunit;

namespace FluxJson.Core.Tests;

public class PerformanceTests
{
    [Fact]
    public void Performance_SerializationSpeed_ShouldBeReasonable()
    {
        // Arrange
        var person = new Person { Name = "John Doe", Age = 30, IsActive = true };
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            var json = Json.From(person).ToJson();
        }

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in under 1 second
        Console.WriteLine($"1000 serializations took: {stopwatch.ElapsedMilliseconds}ms");
    }
}
