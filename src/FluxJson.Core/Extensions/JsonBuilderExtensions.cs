using System;
using FluxJson.Core.Configuration;
using FluxJson.Core.Serialization;

namespace FluxJson.Core.Extensions
{
    public static class JsonBuilderExtensions
    {
        public static JsonBuilder<T> UseCamelCase<T>(this JsonBuilder<T> builder)
        {
            return builder.Configure(config => config.NamingStrategy = NamingStrategy.CamelCase);
        }

        public static JsonBuilder<T> UseSnakeCase<T>(this JsonBuilder<T> builder)
        {
            return builder.Configure(config => config.NamingStrategy = NamingStrategy.SnakeCase);
        }

        public static JsonBuilder<T> IgnoreNulls<T>(this JsonBuilder<T> builder)
        {
            return builder.Configure(config => config.NullHandling = NullHandling.Ignore);
        }

        public static JsonBuilder<T> WriteIndented<T>(this JsonBuilder<T> builder)
        {
            return builder.Configure(config => config.WriteIndented = true);
        }

        public static byte[] ToBytes<T>(this JsonBuilder<T> builder)
        {
            // This is a placeholder. The actual implementation will depend on the JsonWriter.
            return System.Text.Encoding.UTF8.GetBytes(builder.ToJson());
        }

        public static JsonBuilder<T> WithPerformanceMode<T>(this JsonBuilder<T> builder, PerformanceMode mode)
        {
            return builder.Configure(config => config.PerformanceMode = mode);
        }

        public static int ToSpan<T>(this JsonBuilder<T> builder, Span<byte> span)
        {
            var config = builder.GetConfiguration();
            var serializer = new FluxJsonSerializer(config);
            return serializer.SerializeToSpan(builder.GetObject(), span);
        }
    }
}
