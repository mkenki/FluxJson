using System;

namespace FluxJson.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class JsonSerializableAttribute : Attribute
    {
        public JsonSerializableAttribute() { }
    }
}
