using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using FluxJson.Core;
using FluxJson.Core.Extensions;
using Newtonsoft.Json;

namespace FluxJson.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class RealisticScenarioBenchmarks
{
    private List<Order> _orders = null!;
    private string _ordersJson = null!;

    [GlobalSetup]
    public void Setup()
    {
        _orders = GenerateOrders(50);
        _ordersJson = System.Text.Json.JsonSerializer.Serialize(_orders);
    }

    private List<Order> GenerateOrders(int count)
    {
        var random = new Random(42);
        return Enumerable.Range(1, count)
            .Select(i => new Order
            {
                Id = i,
                CustomerName = $"Customer {i}",
                OrderDate = DateTime.Now.AddDays(-random.Next(365)),
                TotalAmount = (decimal)(random.NextDouble() * 1000 + 50),
                IsComplete = random.NextDouble() > 0.3,
                Items = GenerateOrderItems(random.Next(1, 10), random),
                ShippingAddress = new Address
                {
                    Street = $"{random.Next(1, 9999)} Main St",
                    City = "Sample City",
                    State = "ST",
                    ZipCode = $"{random.Next(10000, 99999)}",
                    Country = "USA"
                }
            })
            .ToList();
    }

    private List<OrderItem> GenerateOrderItems(int count, Random random)
    {
        return Enumerable.Range(1, count)
            .Select(i => new OrderItem
            {
                ProductName = $"Product {i}",
                Quantity = random.Next(1, 10),
                UnitPrice = (decimal)(random.NextDouble() * 100 + 10),
                Category = $"Category {random.Next(1, 5)}"
            })
            .ToList();
    }

    [Benchmark(Baseline = true)]
    public string SystemTextJson_SerializeOrders()
    {
        return System.Text.Json.JsonSerializer.Serialize(_orders);
    }

    [Benchmark]
    public string NewtonsoftJson_SerializeOrders()
    {
        return JsonConvert.SerializeObject(_orders);
    }

    [Benchmark]
    public string FluxJson_SerializeOrders()
    {
        return Json.From(_orders).ToJson();
    }

    [Benchmark]
    public string FluxJson_SerializeOrders_CamelCase()
    {
        return Json.From(_orders).UseCamelCase().ToJson();
    }

    [Benchmark]
    public string FluxJson_SerializeOrders_IgnoreNulls()
    {
        return Json.From(_orders).IgnoreNulls().ToJson();
    }

    [Benchmark]
    public List<Order>? SystemTextJson_DeserializeOrders()
    {
        return System.Text.Json.JsonSerializer.Deserialize<List<Order>>(_ordersJson);
    }

    [Benchmark]
    public List<Order>? NewtonsoftJson_DeserializeOrders()
    {
        return JsonConvert.DeserializeObject<List<Order>>(_ordersJson);
    }

    [Benchmark]
    public List<Order>? FluxJson_DeserializeOrders()
    {
        return Json.Parse(_ordersJson).To<List<Order>>();
    }
}

public class Order
{
    public int Id { get; set; }
    public string? CustomerName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsComplete { get; set; }
    public List<OrderItem>? Items { get; set; }
    public Address? ShippingAddress { get; set; }
}

public class OrderItem
{
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Category { get; set; }
}
