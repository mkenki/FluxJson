// src/FluxJson.Benchmarks/SerializationBenchmarks.cs
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluxJson.Core;
using FluxJson.Core.Extensions;
using Newtonsoft.Json;

namespace FluxJson.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SerializationBenchmarks
{
    private Person _person = null!;
    private ComplexObject _complexObject = null!;
    private List<Person> _personList = null!;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            BirthDate = new DateTime(1993, 1, 1),
            Email = "john.doe@example.com"
        };

        _complexObject = new ComplexObject
        {
            StringValue = "Test String",
            IntValue = 42,
            DoubleValue = 3.14159,
            BoolValue = true,
            DateTimeValue = DateTime.Now,
            NestedObject = _person,
            StringList = ["Item1", "Item2", "Item3", "Item4", "Item5"],
            Dictionary = new Dictionary<string, object>
            {
                ["key1"] = "value1",
                ["key2"] = 123,
                ["key3"] = true
            }
        };

        _personList = Enumerable.Range(1, 1000)
            .Select(i => new Person
            {
                Name = $"Person {i}",
                Age = 20 + (i % 50),
                IsActive = i % 2 == 0,
                BirthDate = new DateTime(1970 + (i % 50), 1, 1),
                Email = $"person{i}@example.com"
            })
            .ToList();
    }

    [Benchmark(Baseline = true)]
    public string SystemTextJson_SerializePerson()
    {
        return System.Text.Json.JsonSerializer.Serialize(_person);
    }

    [Benchmark]
    public string NewtonsoftJson_SerializePerson()
    {
        return JsonConvert.SerializeObject(_person);
    }

    [Benchmark]
    public string FluxJson_SerializePerson()
    {
        return Json.From(_person).ToJson();
    }

    [Benchmark]
    public string FluxJson_SerializePerson_Speed()
    {
        return Json.From(_person)
            .WithPerformanceMode(FluxJson.Core.Configuration.PerformanceMode.Speed)
            .ToJson();
    }

    [Benchmark]
    public byte[] FluxJson_SerializePerson_ToBytes()
    {
        return Json.From(_person).ToBytes();
    }

    [Benchmark]
    public string SystemTextJson_SerializeComplexObject()
    {
        return System.Text.Json.JsonSerializer.Serialize(_complexObject);
    }

    [Benchmark]
    public string NewtonsoftJson_SerializeComplexObject()
    {
        return JsonConvert.SerializeObject(_complexObject);
    }

    [Benchmark]
    public string FluxJson_SerializeComplexObject()
    {
        return Json.From(_complexObject).ToJson();
    }

    [Benchmark]
    public string SystemTextJson_SerializeList()
    {
        return System.Text.Json.JsonSerializer.Serialize(_personList);
    }

    [Benchmark]
    public string NewtonsoftJson_SerializeList()
    {
        return JsonConvert.SerializeObject(_personList);
    }

    [Benchmark]
    public string FluxJson_SerializeList()
    {
        return Json.From(_personList).ToJson();
    }

    // Zero-allocation benchmarks
    [Benchmark]
    public int FluxJson_SerializeToSpan()
    {
        Span<byte> buffer = stackalloc byte[2048];
        return Json.From(_person).ToSpan(buffer);
    }
}
