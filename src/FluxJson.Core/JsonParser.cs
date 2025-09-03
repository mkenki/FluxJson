using System;
using FluxJson.Core.Configuration;

namespace FluxJson.Core
{
    public class JsonParser
    {
        private readonly string? _jsonString;
        private readonly byte[]? _jsonBytes;
        private JsonConfiguration _config;

        public JsonParser(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }
            _jsonString = json;
            _config = new JsonConfiguration();
        }

        public JsonParser(ReadOnlySpan<byte> jsonBytes)
        {
            if (jsonBytes.IsEmpty)
            {
                throw new ArgumentException("JSON bytes cannot be empty.", nameof(jsonBytes));
            }
            _jsonBytes = jsonBytes.ToArray(); // Convert ReadOnlySpan<byte> to byte[]
            _config = new JsonConfiguration();
        }

        public JsonParser Configure(Action<JsonConfiguration> configAction)
        {
            configAction(_config);
            return this;
        }

        public T To<T>()
        {
            var serializer = new FluxJsonSerializer(_config);
            if (_jsonString != null)
            {
                return serializer.Deserialize<T>(_jsonString)!;
            }
            else if (_jsonBytes != null)
            {
                return serializer.Deserialize<T>(_jsonBytes)!;
            }
            throw new InvalidOperationException("No JSON data provided to parse.");
        }
    }
}
