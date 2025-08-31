using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using FluxJson.Core.Configuration;
using FluxJson.Core.Converters;
using static FluxJson.Core.TypeHelpers;

namespace FluxJson.Core.Serialization;

public static class JsonSerializationLogic
{
    public static void WriteValue<T>(ref JsonWriter writer, T value, Type type, JsonConfiguration config, Func<Type, PropertyInfo?, (bool, IJsonConverter?)> tryGetConverter, PropertyInfo? property = null)
    {
        if (value is null)
        {
            if (config.NullHandling != NullHandling.Ignore)
                writer.WriteNull();
            return;
        }

        // Check for custom converters first
        if (tryGetConverter(type, property).Item1)
        {
            var (found, converter) = tryGetConverter(type, property);
            if (found && converter is IJsonConverter<T> typedConverter && typedConverter.CanWrite)
            {
                typedConverter.Write(ref writer, value, config);
                return;
            }
        }

        // Handle primitive types
        switch (Type.GetTypeCode(type))
        {
            case TypeCode.String:
                writer.WriteValue((string)(object)value);
                break;
            case TypeCode.Boolean:
                writer.WriteValue((bool)(object)value);
                break;
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
                writer.WriteValue(Convert.ToInt32(value));
                break;
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
                writer.WriteValue(Convert.ToInt64(value));
                break;
            case TypeCode.Single:
                writer.WriteValue(Convert.ToSingle(value));
                break;
            case TypeCode.Double:
                writer.WriteValue(Convert.ToDouble(value));
                break;
            case TypeCode.Decimal:
                writer.WriteValue(Convert.ToDecimal(value));
                break;
            case TypeCode.DateTime:
                WriteDateTime(ref writer, (DateTime)(object)value, config);
                break;
            default:
                WriteComplexValue(ref writer, value, type, config, tryGetConverter);
                break;
        }
    }

    private static void WriteComplexValue<T>(ref JsonWriter writer, T value, Type type, JsonConfiguration config, Func<Type, PropertyInfo?, (bool, IJsonConverter?)> tryGetConverter)
    {
        // Handle nullable types
        if (IsNullableType(type))
        {
            if (value is not null)
            {
                var underlyingType = GetNullableUnderlyingType(type);
                WriteValue(ref writer, value, underlyingType, config, tryGetConverter, null);
            }
            else
            {
                writer.WriteNull();
            }
            return;
        }

        // Handle arrays and collections
        if (IsCollectionType(type) && value is IEnumerable enumerable)
        {
            WriteArray(ref writer, enumerable, type, config, tryGetConverter);
            return;
        }

        // Handle dictionaries
        if (value is IDictionary dictionary)
        {
            WriteDictionary(ref writer, dictionary, config, tryGetConverter);
            return;
        }

        // Handle Guid
        if (type == typeof(Guid))
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(((Guid)(object)value).ToString());
            }
            return;
        }

        // Handle DateTimeOffset
        if (type == typeof(DateTimeOffset))
        {
            if (value is not null)
                WriteDateTimeOffset(ref writer, (DateTimeOffset)(object)value, config);
            else
                writer.WriteNull();
            return;
        }

        // Handle enums
        if (type.IsEnum)
        {
            WriteEnum(ref writer, value, type);
            return;
        }

        // Handle objects
        WriteObject(ref writer, value, type, config, tryGetConverter);
    }

    private static void WriteArray(ref JsonWriter writer, IEnumerable enumerable, Type type, JsonConfiguration config, Func<Type, PropertyInfo?, (bool, IJsonConverter?)> tryGetConverter)
    {
        writer.WriteStartArray();

        var isFirst = true;
        var elementType = GetCollectionElementType(type);

        foreach (var item in enumerable)
        {
            if (!isFirst)
                writer.WriteSeparator();

            WriteValue(ref writer, item, elementType, config, tryGetConverter, property: null);
            isFirst = false;
        }

        writer.WriteEndArray();
    }

    private static void WriteDictionary(ref JsonWriter writer, IDictionary dictionary, JsonConfiguration config, Func<Type, PropertyInfo?, (bool, IJsonConverter?)> tryGetConverter)
    {
        writer.WriteStartObject();

        var isFirst = true;
        foreach (DictionaryEntry entry in dictionary)
        {
            if (!isFirst)
                writer.WriteSeparator();

            var keyString = entry.Key?.ToString() ?? "null";
            writer.WritePropertyName(keyString);
            WriteValue(ref writer, entry.Value, entry.Value?.GetType() ?? typeof(object), config, tryGetConverter, property: null);
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    private static void WriteObject<T>(ref JsonWriter writer, T value, Type type, JsonConfiguration config, Func<Type, PropertyInfo?, (bool, IJsonConverter?)> tryGetConverter)
    {
        writer.WriteStartObject();

        var properties = GetSerializableProperties(type, config);
        var isFirst = true;

        foreach (var property in properties)
        {
            var propValue = property.GetValue(value);

            // Handle null values based on configuration
            if (propValue is null && config.NullHandling == NullHandling.Ignore)
                continue;

            if (!isFirst)
                writer.WriteSeparator();

            var propertyName = GetPropertyName(property.Name, config);
            writer.WritePropertyName(propertyName);

            WriteValue(ref writer, propValue, property.PropertyType, config, tryGetConverter, property);
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    private static void WriteDateTime(ref JsonWriter writer, DateTime dateTime, JsonConfiguration config)
    {
        var formatted = config.DateTimeFormat switch
        {
            DateTimeFormat.ISO8601 => dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture),
            DateTimeFormat.UnixTimestamp => ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            DateTimeFormat.Custom => dateTime.ToString(config.CustomDateTimeFormat, CultureInfo.InvariantCulture),
            _ => dateTime.ToString("O", CultureInfo.InvariantCulture)
        };

        writer.WriteValue(formatted);
    }

    private static void WriteDateTimeOffset(ref JsonWriter writer, DateTimeOffset dateTimeOffset, JsonConfiguration config)
    {
        var formatted = config.DateTimeFormat switch
        {
            DateTimeFormat.ISO8601 => dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture),
            DateTimeFormat.UnixTimestamp => dateTimeOffset.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            _ => dateTimeOffset.ToString("O", CultureInfo.InvariantCulture)
        };

        writer.WriteValue(formatted);
    }

    private static void WriteEnum<T>(ref JsonWriter writer, T value, Type type)
    {
        var enumString = value?.ToString() ?? "null";
        writer.WriteValue(enumString);
    }

    private static PropertyInfo[] GetSerializableProperties(Type type, JsonConfiguration config)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !config.IgnoreReadOnlyProperties || p.CanWrite)
            .ToArray();
    }

    private static string GetPropertyName(string originalName, JsonConfiguration config)
    {
        return config.NamingStrategy switch
        {
            NamingStrategy.CamelCase => ToCamelCase(originalName),
            NamingStrategy.SnakeCase => ToSnakeCase(originalName),
            NamingStrategy.KebabCase => ToKebabCase(originalName),
            _ => originalName
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var result = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
                result.Append('_');
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    private static string ToKebabCase(string name)
    {
        return ToSnakeCase(name).Replace('_', '-');
    }
}
