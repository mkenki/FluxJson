// tests/FluxJson.Core.Tests/TestModels.cs
namespace FluxJson.Core.Tests;

public class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime? BirthDate { get; set; }
}

public class PersonData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
}

public class ComplexObject
{
    public string? StringValue { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public bool BoolValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public Person? NestedObject { get; set; }
    public List<string>? StringList { get; set; }
    public Dictionary<string, object>? Dictionary { get; set; }
}
