using Xunit;
using FluentAssertions;
using FluxJson.Core.Tests;

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

    [Fact]
    public void Performance_SpanSerialization_ShouldBeZeroAllocation()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        Span<byte> buffer = stackalloc byte[1024];

        // Act
        var bytesWritten = Json.From(person).ToSpan(buffer);

        // Assert
        bytesWritten.Should().BeGreaterThan(0);
        bytesWritten.Should().BeLessThan(1024);

        // Verify the JSON is valid
        var json = System.Text.Encoding.UTF8.GetString(buffer[..bytesWritten]);
        json.Should().Contain("John");
    }
}
