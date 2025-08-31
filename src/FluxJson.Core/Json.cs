// src/FluxJson.Core/Json.cs
using System;
using System.Reflection;
using FluxJson.Core;
using FluxJson.Core.Serialization;

namespace FluxJson
{
    public static class Json
    {
        public static JsonBuilder<T> From<T>(T obj)
        {
            // Eğer Source Generator kodu yoksa reflection kullan
            if (!IsSourceGeneratedType<T>())
            {
                return new ReflectionBasedJsonBuilder<T>(obj);
            }
            return new DefaultJsonBuilder<T>(obj);
        }

        public static Core.JsonParser Parse(string json)
        {
            return new Core.JsonParser(json);
        }

        private static bool IsSourceGeneratedType<T>()
        {
            var type = typeof(T);

            // Anonymous type kontrolü
            if (IsAnonymousType(type))
                return false;

            // Source Generator tarafından işlenmiş tip kontrolü
            // Source Generator'ınız types'lara özel attribute ekler
            return type.GetCustomAttribute<FluxJsonGeneratedAttribute>() != null;
        }

        private static bool IsAnonymousType(Type type)
        {
            return type.Name.Contains("AnonymousType") ||
                   (type.IsGenericType && type.Name.StartsWith("<>"));
        }

        public static T FromJsonSerializable<T>(global::FluxJson.Core.JsonReader reader) where T : class, new()
        {
            T instance = new T();
            // TODO: Implement deserialization logic here
            return instance;
        }

        public static string Serialize<T>(T obj)
        {
            return From(obj).ToJson();
        }

        public static T Deserialize<T>(string json)
        {
            return Parse(json).To<T>();
        }
    }
}
