// src/FluxJson.Core/IJsonSerializer.cs
using System.Text;

namespace FluxJson.Core;

/// <summary>
/// Core JSON serialization interface
/// </summary>
public interface IJsonSerializer
{
    /// <summary>
    /// Serialize object to JSON string
    /// </summary>
    string Serialize<T>(T value);

    /// <summary>
    /// Serialize object to JSON bytes
    /// </summary>
    byte[] SerializeToBytes<T>(T value);

    /// <summary>
    /// Serialize object to UTF8 span
    /// </summary>
    int SerializeToSpan<T>(T value, Span<byte> destination);

    /// <summary>
    /// Deserialize JSON string to object
    /// </summary>
    T? Deserialize<T>(string json);

    /// <summary>
    /// Deserialize JSON bytes to object
    /// </summary>
    T? Deserialize<T>(ReadOnlySpan<byte> json);

    /// <summary>
    /// Deserialize JSON to object with type
    /// </summary>
    object? Deserialize(string json, Type type);

    /// <summary>
    /// Async serialize to stream
    /// </summary>
    Task SerializeAsync<T>(T value, Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Async deserialize from stream
    /// </summary>
    Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default);
}