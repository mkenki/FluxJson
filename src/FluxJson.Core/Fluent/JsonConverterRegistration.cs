using System.Linq.Expressions;
using System.Reflection;
using FluxJson.Core.Configuration;
using FluxJson.Core.Converters;

namespace FluxJson.Core.Fluent;

/// <summary>
/// Implements the fluent interface for registering JSON converters for specific types or properties.
/// </summary>
/// <typeparam name="T">The type for which converters are being registered.</typeparam>
public class JsonConverterRegistration<T> : IJsonConverterRegistration<T>
{
    private readonly JsonConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConverterRegistration{T}"/> class.
    /// </summary>
    /// <param name="configuration">The JSON configuration to apply converter registrations to.</param>
    public JsonConverterRegistration(JsonConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public IJsonConverterRegistration<T> UseConverter<TConverter>() where TConverter : IJsonConverter, new()
    {
        _configuration.RegisterTypeConverter(typeof(T), new TConverter());
        return this;
    }

    /// <inheritdoc />
    public IJsonConverterRegistration<T> ForProperty<TProperty, TConverter>(Expression<Func<T, TProperty>> propertySelector)
        where TConverter : IJsonConverter, new()
    {
        if (propertySelector.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Property selector must be a member expression.", nameof(propertySelector));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Property selector must select a property.", nameof(propertySelector));
        }

        _configuration.RegisterPropertyConverter(typeof(T), propertyInfo.Name, new TConverter());
        return this;
    }
}
