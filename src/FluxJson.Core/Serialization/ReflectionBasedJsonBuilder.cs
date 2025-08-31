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
        private JsonConfiguration _config;

        public ReflectionBasedJsonBuilder(T obj) : base(obj)
        {
            _obj = obj;
            _config = new JsonConfiguration();
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

            return SerializeObject(_obj, _config);
        }

        private string SerializeObject(object obj, JsonConfiguration config)
        {
            if (obj == null)
                return "null";

            var type = obj.GetType();

            // Primitive types
            if (IsPrimitiveType(type))
                return SerializePrimitive(obj);

            // Complex objects
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanRead);

            var jsonPairs = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);

                // Null handling
                if (value == null && config.NullHandling == NullHandling.Ignore)
                    continue;

                var propertyName = GetPropertyName(prop.Name, config.NamingStrategy);
                var serializedValue = SerializeValue(value, config);

                jsonPairs.Add($"\"{propertyName}\":{serializedValue}");
            }

            return "{" + string.Join(",", jsonPairs) + "}";
        }

        private string SerializeValue(object value, JsonConfiguration config)
        {
            if (value == null)
                return "null";

            var type = value.GetType();

            if (IsPrimitiveType(type))
                return SerializePrimitive(value);

            if (type == typeof(DateTime))
                return $"\"{((DateTime)value).ToString(config.CustomDateTimeFormat ?? "yyyy-MM-ddTHH:mm:ss.fffffffZ")}\"";

            // Recursive serialization for complex objects
            return SerializeObject(value, config);
        }

        private bool IsPrimitiveType(Type type)
        {
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        private string SerializePrimitive(object value)
        {
            return value switch
            {
                string s => $"\"{EscapeString(s)}\"",
                bool b => b.ToString().ToLower(),
                char c => $"\"{EscapeString(c.ToString())}\"",
                DateTime dt => $"\"{dt:yyyy-MM-ddTHH:mm:ss.fffffffZ}\"",
                Guid g => $"\"{g}\"",
                null => "null",
                _ => Convert.ToString(value, CultureInfo.InvariantCulture)
            };
        }

        private string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var escaped = new StringBuilder();
            foreach (char c in str)
            {
                switch (c)
                {
                    case '"': escaped.Append("\\\""); break;
                    case '\\': escaped.Append("\\\\"); break;
                    case '\b': escaped.Append("\\b"); break;
                    case '\f': escaped.Append("\\f"); break;
                    case '\n': escaped.Append("\\n"); break;
                    case '\r': escaped.Append("\\r"); break;
                    case '\t': escaped.Append("\\t"); break;
                    default:
                        if (c < 32)
                            escaped.Append($"\\u{(int)c:x4}");
                        else
                            escaped.Append(c);
                        break;
                }
            }
            return escaped.ToString();
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
                return input?.ToLowerInvariant();

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        private string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

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
            return ToSnakeCase(input)?.Replace('_', '-');
        }
    }
}
