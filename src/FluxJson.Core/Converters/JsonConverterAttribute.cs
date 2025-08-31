using System;

namespace FluxJson.Core.Converters;

/// <summary>
/// Specifies the <see cref="IJsonConverter"/> to use for a type or property.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonConverterAttribute : Attribute
{
    /// <summary>
    /// Gets the type of the converter.
    /// </summary>
    public Type ConverterType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterAttribute"/> class.
    /// </summary>
    /// <param name="converterType">The type of the converter to use. Must implement <see cref="IJsonConverter"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="converterType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="converterType"/> does not implement <see cref="IJsonConverter"/>.</exception>
    public JsonConverterAttribute(Type converterType)
    {
        if (converterType is null)
        {
            throw new ArgumentNullException(nameof(converterType));
        }

        if (!typeof(IJsonConverter).IsAssignableFrom(converterType))
        {
            throw new ArgumentException($"Converter type '{converterType.FullName}' must implement IJsonConverter.", nameof(converterType));
        }

        ConverterType = converterType;
    }
}
