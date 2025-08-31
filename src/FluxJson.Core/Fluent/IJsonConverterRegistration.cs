namespace FluxJson.Core.Fluent;

/// <summary>
/// Fluent interface for registering JSON converters for specific types or properties.
/// </summary>
/// <typeparam name="T">The type for which converters are being registered.</typeparam>
public interface IJsonConverterRegistration<T>
{
    /// <summary>
    /// Registers a custom converter for the specified type.
    /// </summary>
    /// <typeparam name="TConverter">The type of the custom converter.</typeparam>
    /// <returns>The current converter registration builder.</returns>
    IJsonConverterRegistration<T> UseConverter<TConverter>() where TConverter : IJsonConverter, new();

    /// <summary>
    /// Registers a custom converter for a specific property of the type.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <typeparam name="TConverter">The type of the custom converter.</typeparam>
    /// <param name="propertySelector">An expression to select the property.</param>
    /// <returns>The current converter registration builder.</returns>
    IJsonConverterRegistration<T> ForProperty<TProperty, TConverter>(System.Linq.Expressions.Expression<Func<T, TProperty>> propertySelector)
        where TConverter : IJsonConverter, new();
}
