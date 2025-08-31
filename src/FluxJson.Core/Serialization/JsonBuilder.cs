// 4. JsonBuilder.cs - Base class'ınızı güncelleyin (eğer yoksa)
using System;
using FluxJson.Core.Configuration;

namespace FluxJson.Core.Serialization
{
    public abstract class JsonBuilder<T>
    {
        protected readonly T _object;
        protected readonly JsonConfiguration _config;

        protected JsonBuilder(T obj, JsonConfiguration? config = null)
        {
            _object = obj;
            _config = config ?? new JsonConfiguration();
        }

        public abstract JsonBuilder<T> Configure(Action<JsonConfiguration> configAction);
        public abstract string ToJson();

        // ToSpan extension methodunun _config alanına erişebilmesi için bu metodu ekliyoruz.
        public JsonConfiguration GetConfiguration() => _config;
    }
}
