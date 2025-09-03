// 3. ReflectionBasedJsonBuilder.cs - Anonymous type'lar için reflection serializer
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using FluxJson.Core.Configuration;

namespace FluxJson.Core.Serialization
{
    public class ReflectionBasedJsonBuilder<T> : JsonBuilder<T>
    {
        private readonly T _obj;

        public ReflectionBasedJsonBuilder(T obj) : base(obj)
        {
            _obj = obj!;
        }

        public override JsonBuilder<T> Configure(Action<JsonConfiguration> configAction)
        {
            configAction(_config);
            return this;
        }

        public override string ToJson()
        {
            if (_obj == null)
                return "null";

            // Use a temporary buffer for serialization
            var buffer = new byte[4096]; // Adjust size as needed
            var writer = new JsonWriter(buffer.AsSpan(), _config);
            SerializeObject(_obj, ref writer, _config);
            return Encoding.UTF8.GetString(writer.WrittenSpan);
        }

        private void SerializeObject(object obj, ref JsonWriter writer, JsonConfiguration config)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }

            var type = obj.GetType();

            // Primitive types
            if (IsPrimitiveType(type))
            {
                SerializePrimitive(obj, ref writer);
                return;
            }

            writer.WriteStartObject();

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanRead);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);

                // Null handling
                if (value == null && config.NullHandling == NullHandling.Ignore)
                    continue;

                var propertyName = GetPropertyName(prop.Name, config.NamingStrategy);
                writer.WritePropertyName(propertyName.AsSpan());
                // Fixed: Handle null value properly
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    SerializeValue(value, ref writer, config);
                }
            }

            writer.WriteEndObject();
        }

        private void SerializeValue(object value, ref JsonWriter writer, JsonConfiguration config)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var type = value.GetType();

            if (IsPrimitiveType(type))
            {
                SerializePrimitive(value, ref writer);
                return;
            }

            if (type == typeof(DateTime))
            {
                writer.WriteValue(((DateTime)value).ToString(config.CustomDateTimeFormat ?? "O")); // ISO 8601 format
                return;
            }

            // Recursive serialization for complex objects
            SerializeObject(value, ref writer, config);
        }

        private bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        private void SerializePrimitive(object value, ref JsonWriter writer)
        {
            switch (value)
            {
                case string s:
                    writer.WriteValue(s);
                    break;
                case bool b:
                    writer.WriteValue(b);
                    break;
                case char c:
                    writer.WriteValue(c.ToString());
                    break;
                case DateTime dt:
                    writer.WriteValue(dt.ToString("O")); // ISO 8601 format
                    break;
                case Guid g:
                    writer.WriteValue(g.ToString());
                    break;
                case int i:
                    writer.WriteValue(i);
                    break;
                case long l:
                    writer.WriteValue(l);
                    break;
                case float f:
                    writer.WriteValue(f);
                    break;
                case double d:
                    writer.WriteValue(d);
                    break;
                case decimal dec:
                    writer.WriteValue(dec);
                    break;
                default:
                    writer.WriteNull(); // Should not happen for primitive types
                    break;
            }
        }

        private string GetPropertyName(string propertyName, NamingStrategy namingStrategy)
        {
            return namingStrategy switch
            {
                NamingStrategy.CamelCase => ToCamelCase(propertyName),
                NamingStrategy.SnakeCase => ToSnakeCase(propertyName),
                NamingStrategy.KebabCase => ToKebabCase(propertyName),
                _ => propertyName // PascalCase (default)
            };
        }

        private string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length == 1)
                return input?.ToLowerInvariant() ?? string.Empty; // Fixed: Handle null properly

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty; // Fixed: Handle null properly

            var result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i > 0 && char.IsUpper(input[i]))
                    result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            return result.ToString();
        }

        private string ToKebabCase(string input)
        {
            return ToSnakeCase(input).Replace('_', '-'); // Fixed: Removed null-forgiving operator
        }
    }
}
