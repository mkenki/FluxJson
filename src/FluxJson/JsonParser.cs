using System;
using FluxJson.Core.Configuration;

namespace FluxJson.Core
{
    public class JsonParser
    {
        private readonly string _json;
        private JsonConfiguration _config;

        public JsonParser(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }
            _json = json;
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
            return serializer.Deserialize<T>(_json);
        }
    }
}
