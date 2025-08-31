// src/FluxJson.Core/Extensions/StringExtensions.cs
namespace FluxJson.Core.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Converts the input string to camelCase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The camelCase string.</returns>
    public static string ToCamelCase(this string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
            return input;

        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    /// <summary>
    /// Converts the input string to snake_case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The snake_case string.</returns>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && i > 0)
                result.Append('_');
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    /// <summary>
    /// Converts the input string to kebab-case.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The kebab-case string.</returns>
    public static string ToKebabCase(this string input)
    {
        return input.ToSnakeCase().Replace('_', '-');
    }
}
