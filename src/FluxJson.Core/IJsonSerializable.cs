namespace FluxJson.Core
{
    public interface IJsonSerializable<T> where T : IJsonSerializable<T>
    {
        void ToJson(ref JsonWriter writer);
        static abstract T FromJson(JsonReader reader);
    }
}
