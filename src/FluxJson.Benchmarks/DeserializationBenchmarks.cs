// src/FluxJson.Benchmarks/DeserializationBenchmarks.cs
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluxJson.Core;
using FluxJson.Core.Extensions;
using Newtonsoft.Json;

namespace FluxJson.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class DeserializationBenchmarks
{
    private string _personJson = null!;
    private string _complexObjectJson = null!;
    private string _personListJson = null!;
    private byte[] _personJsonBytes = null!;

    [GlobalSetup]
    public void Setup()
    {
        var person = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            BirthDate = new DateTime(1993, 1, 1),
            Email = "john.doe@example.com"
        };

        var complexObject = new ComplexObject
        {
            StringValue = "Test String",
            IntValue = 42,
            DoubleValue = 3.14159,
            BoolValue = true,
            DateTimeValue = DateTime.Now,
            NestedObject = person,
            StringList = ["Item1", "Item2", "Item3"],
            Dictionary = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 123
            }
        };

        var personList = Enumerable.Range(1, 100)
            .Select(i => new Person
            {
                Name = $"Person {i}",
                Age = 20 + (i % 50),
                IsActive = i % 2 == 0,
                Email = $"person{i}@example.com"
            })
            .ToList();

        _personJson = System.Text.Json.JsonSerializer.Serialize(person);
        _complexObjectJson = System.Text.Json.JsonSerializer.Serialize(complexObject);
        _personListJson = System.Text.Json.JsonSerializer.Serialize(personList);
        _personJsonBytes = System.Text.Encoding.UTF8.GetBytes(_personJson);
    }

    [Benchmark(Baseline = true)]
    public Person? SystemTextJson_DeserializePerson()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Person>(_personJson);
    }

    [Benchmark]
    public Person? NewtonsoftJson_DeserializePerson()
    {
        return JsonConvert.DeserializeObject<Person>(_personJson);
    }

    [Benchmark]
    public Person? FluxJson_DeserializePerson()
    {
        return Json.Parse(_personJson).To<Person>();
    }

    [Benchmark]
    public Person? FluxJson_DeserializePerson_Speed()
    {
        return Json.Parse(_personJson)
            .Configure(config => config.WithPerformanceMode(FluxJson.Core.Configuration.PerformanceMode.Speed))
            .To<Person>();
    }

    [Benchmark]
    public Person? FluxJson_DeserializePerson_FromBytes()
    {
        return Json.Parse(_personJsonBytes.AsSpan()).To<Person>();
    }

    [Benchmark]
    public ComplexObject? SystemTextJson_DeserializeComplexObject()
    {
        return System.Text.Json.JsonSerializer.Deserialize<ComplexObject>(_complexObjectJson);
    }

    [Benchmark]
    public ComplexObject? NewtonsoftJson_DeserializeComplexObject()
    {
        return JsonConvert.DeserializeObject<ComplexObject>(_complexObjectJson);
    }

    [Benchmark]
    public ComplexObject? FluxJson_DeserializeComplexObject()
    {
        return Json.Parse(_complexObjectJson).To<ComplexObject>();
    }

    [Benchmark]
    public List<Person>? SystemTextJson_DeserializeList()
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<Person>>(_personListJson);
    }

    [Benchmark]
    public List<Person>? NewtonsoftJson_DeserializeList()
    {
        return JsonConvert.DeserializeObject<List<Person>>(_personListJson);
    }

    [Benchmark]
    public List<Person>? FluxJson_DeserializeList()
    {
        return Json.Parse(_personListJson).To<List<Person>>();
    }
}
