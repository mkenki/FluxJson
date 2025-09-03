using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluxJson.Core;
using FluxJson.Core.Configuration;
using FluxJson.Core.Extensions;

namespace FluxJson.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class ConfigurationBenchmarks
{
    private Person _person = null!;
    private PersonData _personData = null!;

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

        _personData = new PersonData
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com"
        };
    }

    [Benchmark(Baseline = true)]
    public string FluxJson_Default()
    {
        return Json.From(_person).ToJson();
    }

    [Benchmark]
    public string FluxJson_CamelCase()
    {
        return Json.From(_person).UseCamelCase().ToJson();
    }

    [Benchmark]
    public string FluxJson_SnakeCase()
    {
        return Json.From(_personData).UseSnakeCase().ToJson();
    }

    [Benchmark]
    public string FluxJson_IgnoreNulls()
    {
        return Json.From(_person).IgnoreNulls().ToJson();
    }

    [Benchmark]
    public string FluxJson_Indented()
    {
        return Json.From(_person).WriteIndented().ToJson();
    }

    [Benchmark]
    public string FluxJson_SpeedMode()
    {
        return Json.From(_person)
            .WithPerformanceMode(FluxJson.Core.Configuration.PerformanceMode.Speed)
            .ToJson();
    }

    [Benchmark]
    public string FluxJson_AllFeatures()
    {
        return Json.From(_person)
            .UseCamelCase()
            .IgnoreNulls()
            .WriteIndented()
            .WithPerformanceMode(FluxJson.Core.Configuration.PerformanceMode.Features)
            .ToJson();
    }

    [Benchmark]
    public string FluxJson_FluentConfiguration()
    {
        return Json.From(_person)
            .Configure(config => config
                .UseNaming(FluxJson.Core.Configuration.NamingStrategy.CamelCase)
                .HandleNulls(FluxJson.Core.Configuration.NullHandling.Ignore)
                .WriteIndented(true))
            .ToJson();
    }
}
