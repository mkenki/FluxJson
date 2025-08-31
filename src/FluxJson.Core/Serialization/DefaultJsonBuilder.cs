using System;

namespace FluxJson.Core.Serialization
{
    public class DefaultJsonBuilder<T> : JsonBuilder<T>
    {
        private readonly T _obj;

        public DefaultJsonBuilder(T obj) : base(obj)
        {
            _obj = obj;
        }

        public override string ToJson()
        {
            return _obj?.ToString() ?? "null";
        }

        public override JsonBuilder<T> Configure(Action<JsonConfiguration> configAction)
        {
            // Default implementation does nothing
            return this;
        }
    }
}
