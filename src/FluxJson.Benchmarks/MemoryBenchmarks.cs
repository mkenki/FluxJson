using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluxJson;

namespace FluxJson.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MemoryBenchmarks
{
    private Person _person = null!;
    private string _personJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            Email = "john.doe@example.com"
        };

        _personJson = System.Text.Json.JsonSerializer.Serialize(_person);
    }

    [Benchmark(Baseline = true)]
    public string SystemTextJson_Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(_person);
    }

    [Benchmark]
    public string FluxJson_Serialize()
    {
        return Json.From(_person).ToJson();
    }

    [Benchmark]
    public byte[] FluxJson_SerializeToBytes()
    {
        return Json.From(_person).ToBytes();
    }

    [Benchmark]
    public Person? SystemTextJson_Deserialize()
    {
        return System.Text.Json.JsonSerializer.Deserialize<Person>(_personJson);
    }

    [Benchmark]
    public Person? FluxJson_Deserialize()
    {
        return Json.Parse(_personJson).To<Person>();
    }

    // Zero allocation scenarios
    [Benchmark]
    public unsafe int FluxJson_SerializeToSpan_Unsafe()
    {
        Span<byte> buffer = stackalloc byte[1024];
        return Json.From(_person).ToSpan(buffer);
    }

    [Benchmark]
    public int FluxJson_SerializeToSpan_Safe()
    {
        var buffer = new byte[1024];
        return Json.From(_person).ToSpan(buffer);
    }
}
