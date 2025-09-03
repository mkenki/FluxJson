using System;
using System.Collections.Generic;

namespace FluxJson.Core
{
    // [JsonSerializable]
    public partial class TestClassForGenerator // : IJsonSerializable<TestClassForGenerator>
    {
        // Force regeneration
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public double Value { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UniqueId { get; set; }
        public MyEnum Status { get; set; }
        public int[]? IntArray { get; set; }
        public string[]? StringArray { get; set; }
        public List<string>? StringList { get; set; }
        public Dictionary<string, int>? StringIntDictionary { get; set; }
        public NestedClass? NestedObject { get; set; }
        public NestedClass[]? NestedObjectArray { get; set; }

    }

    public enum MyEnum
    {
        None,
        Active,
        Inactive
    }

    // [JsonSerializable]
    public partial class NestedClass // : IJsonSerializable<NestedClass>
    {
        public string? NestedName { get; set; }
        public int NestedValue { get; set; }
    }
}
