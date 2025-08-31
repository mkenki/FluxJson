// src/FluxJson.Benchmarks/Program.cs
using BenchmarkDotNet.Running;
using FluxJson.Benchmarks;

BenchmarkRunner.Run<SerializationBenchmarks>();
BenchmarkRunner.Run<DeserializationBenchmarks>();