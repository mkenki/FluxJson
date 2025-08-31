using System;
using System.Collections.Generic;
using FluxJson.Core;
using FluxJson.Core.Configuration;
using Xunit;

namespace FluxJson.Core.Tests
{
    public class SourceGeneratorTests
    {
        [Fact]
        public void TestClassForGenerator_SerializationAndDeserialization()
        {
            var original = new TestClassForGenerator
            {
                Id = 1,
                Name = "Test Name",
                IsActive = true,
                Value = 123.45,
                CreatedDate = new DateTime(2023, 1, 1, 10, 30, 0, DateTimeKind.Utc),
                UniqueId = Guid.Parse("a1b2c3d4-e5f6-7890-1234-567890abcdef"),
                Status = MyEnum.Active,
                IntArray = new int[] { 1, 2, 3 },
                StringArray = new string[] { "one", "two", "three" },
                StringList = new List<string> { "alpha", "beta" },
                StringIntDictionary = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } },
                NestedObject = new NestedClass { NestedName = "Inner", NestedValue = 99 },
                NestedObjectArray = new NestedClass[]
                {
                    new NestedClass { NestedName = "Inner1", NestedValue = 101 },
                    new NestedClass { NestedName = "Inner2", NestedValue = 102 }
                }
            };

            var buffer = new byte[4096]; // Provide a sufficiently large buffer
            var config = new JsonConfiguration();
            var writer = new JsonWriter(buffer.AsSpan(), config);
            original.ToJson(ref writer);
            string json = System.Text.Encoding.UTF8.GetString(writer.WrittenSpan);

            var reader = new JsonReader(System.Text.Encoding.UTF8.GetBytes(json), config);
            var deserialized = TestClassForGenerator.FromJson(reader);

            Assert.NotNull(deserialized);
            Assert.Equal(original.Id, deserialized.Id);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.IsActive, deserialized.IsActive);
            Assert.Equal(original.Value, deserialized.Value);
            Assert.Equal(original.CreatedDate, deserialized.CreatedDate);
            Assert.Equal(original.UniqueId, deserialized.UniqueId);
            Assert.Equal(original.Status, deserialized.Status);
            Assert.Equal(original.IntArray, deserialized.IntArray);
            Assert.Equal(original.StringArray, deserialized.StringArray);
            Assert.Equal(original.StringList, deserialized.StringList);
            Assert.Equal(original.StringIntDictionary, deserialized.StringIntDictionary);
            Assert.NotNull(deserialized.NestedObject);
            Assert.Equal(original.NestedObject.NestedName, deserialized.NestedObject.NestedName);
            Assert.Equal(original.NestedObject.NestedValue, deserialized.NestedObject.NestedValue);
            Assert.NotNull(deserialized.NestedObjectArray);
            Assert.Equal(original.NestedObjectArray.Length, deserialized.NestedObjectArray.Length);
            Assert.Equal(original.NestedObjectArray[0].NestedName, deserialized.NestedObjectArray[0].NestedName);
            Assert.Equal(original.NestedObjectArray[0].NestedValue, deserialized.NestedObjectArray[0].NestedValue);
            Assert.Equal(original.NestedObjectArray[1].NestedName, deserialized.NestedObjectArray[1].NestedName);
            Assert.Equal(original.NestedObjectArray[1].NestedValue, deserialized.NestedObjectArray[1].NestedValue);
        }

        [Fact]
        public void TestClassForGenerator_SerializationAndDeserialization_WithNulls()
        {
            var original = new TestClassForGenerator
            {
                Id = 2,
                Name = null,
                IsActive = false,
                Value = 0.0,
                CreatedDate = DateTime.MinValue,
                UniqueId = Guid.Empty,
                Status = MyEnum.None,
                IntArray = null,
                StringArray = null,
                StringList = null,
                StringIntDictionary = null,
                NestedObject = null,
                NestedObjectArray = null
            };

            var buffer = new byte[4096]; // Provide a sufficiently large buffer
            var config = new JsonConfiguration();
            var writer = new JsonWriter(buffer.AsSpan(), config);
            original.ToJson(ref writer);
            string json = System.Text.Encoding.UTF8.GetString(writer.WrittenSpan);

            var reader = new JsonReader(System.Text.Encoding.UTF8.GetBytes(json), config);
            var deserialized = TestClassForGenerator.FromJson(reader);

            Assert.NotNull(deserialized);
            Assert.Equal(original.Id, deserialized.Id);
            Assert.Null(deserialized.Name);
            Assert.Equal(original.IsActive, deserialized.IsActive);
            Assert.Equal(original.Value, deserialized.Value);
            Assert.Equal(original.CreatedDate, deserialized.CreatedDate);
            Assert.Equal(original.UniqueId, deserialized.UniqueId);
            Assert.Equal(original.Status, deserialized.Status);
            Assert.Null(deserialized.IntArray);
            Assert.Null(deserialized.StringArray);
            Assert.Null(deserialized.StringList);
            Assert.Null(deserialized.StringIntDictionary);
            Assert.Null(deserialized.NestedObject);
            Assert.Null(deserialized.NestedObjectArray);
        }
    }
}
