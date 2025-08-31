// src/FluxJson.Benchmarks/Models.cs
namespace FluxJson.Benchmarks;

public class Person
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Email { get; set; }
    public Address? Address { get; set; }
}

public class Address
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
}

public class PersonData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
}

public class ComplexObject
{
    public string? StringValue { get; set; }
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public decimal DecimalValue { get; set; }
    public bool BoolValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public Guid GuidValue { get; set; }
    public Person? NestedObject { get; set; }
    public List<string>? StringList { get; set; }
    public string[]? StringArray { get; set; }
    public Dictionary<string, object>? Dictionary { get; set; }
    public int[]? IntArray { get; set; }
    public List<Person>? PersonList { get; set; }
}
