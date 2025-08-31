using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace FluxJson.Generator
{
    internal static class JsonDeserializationGenerator
    {
        public static void GenerateFromJsonMethod(StringBuilder sb, INamedTypeSymbol classSymbol, ITypeSymbol genericTypeArgument)
        {
            sb.AppendLine($"        public static {genericTypeArgument.ToDisplayString()} FromJson(global::FluxJson.Core.JsonReader reader)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var instance = new {classSymbol.Name}();");
            sb.AppendLine("            reader.ReadStartObject();");
            sb.AppendLine("            reader.SkipWhitespace();"); // Skip initial whitespace after '{'
            sb.AppendLine("            if (reader.Current == (byte)'}')"); // Check for empty object
            sb.AppendLine("            {");
            sb.AppendLine("                reader.ReadEndObject();");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine("                while (true)");
            sb.AppendLine("                {");
            sb.AppendLine("                    string propertyName = reader.ReadPropertyName();");
            sb.AppendLine("                    reader.SkipWhitespace();");
            sb.AppendLine("                    switch (propertyName)");
            sb.AppendLine("                    {");

            foreach (var property in classSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public && p.SetMethod != null && p.SetMethod.DeclaredAccessibility == Accessibility.Public))
            {
                string propertyName = property.Name;
                ITypeSymbol propertyType = property.Type;
                bool isNullable = propertyType.NullableAnnotation == NullableAnnotation.Annotated;
                ITypeSymbol underlyingType = isNullable && propertyType is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? namedType.TypeArguments[0] : propertyType;

                sb.AppendLine($"                    case \"{propertyName}\":");
                if (isNullable)
                {
                    sb.AppendLine("                        if (reader.TryReadNull())");
                    sb.AppendLine("                        {");
                    sb.AppendLine($"                            instance.{propertyName} = null;");
                    sb.AppendLine("                        }");
                    sb.AppendLine("                        else");
                    sb.AppendLine("                        {");
                }

                GenerateFromJsonPropertyValue(sb, underlyingType, propertyName, "reader");

                if (isNullable)
                {
                    sb.AppendLine("                        }");
                }
                sb.AppendLine("                        break;");
            }

            sb.AppendLine("                    default:");
            sb.AppendLine("                        reader.SkipValue();");
            sb.AppendLine("                        break;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    reader.SkipWhitespace();");
            sb.AppendLine("                    if (reader.Current == (byte)',')");
            sb.AppendLine("                    {");
            sb.AppendLine("                        reader.ReadNext();");
            sb.AppendLine("                        reader.SkipWhitespace();"); // Skip whitespace after comma
            sb.AppendLine("                    }");
            sb.AppendLine("                    else if (reader.Current == (byte)'}')");
            sb.AppendLine("                    {");
            sb.AppendLine("                        reader.ReadEndObject();");
            sb.AppendLine("                        break;");
            sb.AppendLine("                    }");
            sb.AppendLine("                    else");
            sb.AppendLine("                    {");
            sb.AppendLine($"                        throw new FluxJson.Core.JsonException(\"Expected ',' or '}}' at position \" + reader.Position);");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("            }"); // Close the else block for non-empty object

            sb.AppendLine("            return instance;");
            sb.AppendLine("        }");
        }

        private static void GenerateFromJsonPropertyValue(StringBuilder sb, ITypeSymbol type, string propertyName, string readerAccessor)
        {
            string indent = "                        ";

            bool isNullableType = type.NullableAnnotation == NullableAnnotation.Annotated;
            ITypeSymbol underlyingType = isNullableType && type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? namedType.TypeArguments[0] : type;

            if (isNullableType)
            {
                sb.AppendLine($"{indent}if ({readerAccessor}.TryReadNull())");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    instance.{propertyName} = default;");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}{{");
                indent += "    ";
            }
            else if (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.Annotated)
            {
                sb.AppendLine($"{indent}if ({readerAccessor}.TryReadNull())");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    throw new FluxJson.Core.JsonException($\"Unexpected null value for non-nullable property '{propertyName}' at position \" + reader.Position);");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}{{");
                indent += "    ";
            }

            if (underlyingType.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadString();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Int32)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadInt32();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Boolean)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadBoolean();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Double)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadDouble();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Single)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadSingle();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Decimal)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = {readerAccessor}.ReadDecimal();");
            }
            else if (underlyingType.SpecialType == SpecialType.System_DateTime)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = DateTime.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture);");
            }
            else if (underlyingType.ToDisplayString() == "System.DateTimeOffset")
            {
                sb.AppendLine($"{indent}instance.{propertyName} = DateTimeOffset.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture);");
            }
            else if (underlyingType.ToDisplayString() == "System.Guid")
            {
                sb.AppendLine($"{indent}instance.{propertyName} = Guid.Parse({readerAccessor}.ReadString()!);");
            }
            else if (underlyingType.TypeKind == TypeKind.Enum)
            {
                sb.AppendLine($"{indent}instance.{propertyName} = Enum.Parse<{underlyingType.ToDisplayString()}>({readerAccessor}.ReadString()!);");
            }
            else if (underlyingType.TypeKind == TypeKind.Class && underlyingType is INamedTypeSymbol namedClassType && namedClassType.AllInterfaces.Any(i => i.IsGenericType && i.ConstructedFrom.ToDisplayString().StartsWith("FluxJson.Core.IJsonSerializable<")))
            {
                sb.AppendLine($"{indent}instance.{propertyName} = Json.FromJsonSerializable<{underlyingType.ToDisplayString()}>({readerAccessor});");
            }
            else if (underlyingType is IArrayTypeSymbol arrayType)
            {
                ITypeSymbol elementType = arrayType.ElementType;
                sb.AppendLine($"{indent}var {propertyName}Array = new System.Collections.Generic.List<{elementType.ToDisplayString()}>();");
                sb.AppendLine($"{indent}{readerAccessor}.ReadStartArray();");
                sb.AppendLine($"{indent}{readerAccessor}.SkipWhitespace();"); // Skip initial whitespace after '['
                sb.AppendLine($"{indent}if ({readerAccessor}.Current == (byte)']')"); // Check for empty array
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    {readerAccessor}.ReadEndArray();");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    while (true)");
                sb.AppendLine($"{indent}    {{");
                GenerateFromJsonValue(sb, elementType, $"{propertyName}Array", indent + "        ", readerAccessor);
                sb.AppendLine($"{indent}        {readerAccessor}.SkipWhitespace();");
                sb.AppendLine($"{indent}        if ({readerAccessor}.Current == (byte)',')");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            {readerAccessor}.ReadNext();");
                sb.AppendLine($"{indent}            {readerAccessor}.SkipWhitespace();"); // Skip whitespace after comma
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}        else if ({readerAccessor}.Current == (byte)']')");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            {readerAccessor}.ReadEndArray();");
                sb.AppendLine($"{indent}            break;");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}        else");
                sb.AppendLine($"{indent}        {{");
                sb.AppendLine($"{indent}            throw new FluxJson.Core.JsonException(\"Expected ',' or ']' at position \" + reader.Position);");
                sb.AppendLine($"{indent}        }}");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine($"{indent}}}"); // Close the else block for non-empty array
                sb.AppendLine($"{indent}instance.{propertyName} = {propertyName}Array.ToArray();");
            }
            else if (underlyingType is INamedTypeSymbol collectionType && collectionType.IsGenericType)
            {
                if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.List<T>" ||
                    collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
                {
                    ITypeSymbol itemType = collectionType.TypeArguments[0];
                    sb.AppendLine($"{indent}var {propertyName}List = new System.Collections.Generic.List<{itemType.ToDisplayString()}>();");
                    sb.AppendLine($"{indent}{readerAccessor}.ReadStartArray();");
                    sb.AppendLine($"{indent}{readerAccessor}.SkipWhitespace();"); // Skip initial whitespace after '['
                    sb.AppendLine($"{indent}if ({readerAccessor}.Current == (byte)']')"); // Check for empty array
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    {readerAccessor}.ReadEndArray();");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}else");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    while (true)");
                    sb.AppendLine($"{indent}    {{");
                    GenerateFromJsonValue(sb, itemType, $"{propertyName}List", indent + "        ", readerAccessor);
                    sb.AppendLine($"{indent}        {readerAccessor}.SkipWhitespace();");
                    sb.AppendLine($"{indent}        if ({readerAccessor}.Current == (byte)',')");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            {readerAccessor}.ReadNext();");
                    sb.AppendLine($"{indent}            {readerAccessor}.SkipWhitespace();"); // Skip whitespace after comma
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}        else if ({readerAccessor}.Current == (byte)']')");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            {readerAccessor}.ReadEndArray();");
                    sb.AppendLine($"{indent}            break;");
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}        else");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            throw new FluxJson.Core.JsonException(\"Expected ',' or ']' at position \" + reader.Position);");
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}}}"); // Close the else block for non-empty array
                    sb.AppendLine($"{indent}instance.{propertyName} = {propertyName}List;");
                }
                else if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
                {
                    ITypeSymbol keyType = collectionType.TypeArguments[0];
                    ITypeSymbol valueType = collectionType.TypeArguments[1];
                    sb.AppendLine($"{indent}var {propertyName}Dictionary = new System.Collections.Generic.Dictionary<{keyType.ToDisplayString()}, {valueType.ToDisplayString()}>();");
                    sb.AppendLine($"{indent}{readerAccessor}.ReadStartObject();");
                    sb.AppendLine($"{indent}{readerAccessor}.SkipWhitespace();"); // Skip initial whitespace after '{'
                    sb.AppendLine($"{indent}if ({readerAccessor}.Current == (byte)'}}')"); // Check for empty dictionary
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    {readerAccessor}.ReadEndObject();");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}else");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    while (true)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        {readerAccessor}.SkipWhitespace();");
                    sb.AppendLine($"{indent}        string key = {readerAccessor}.ReadPropertyName();");
                    string keyConversion = keyType.SpecialType == SpecialType.System_String ? "key" : $"({keyType.ToDisplayString()})Convert.ChangeType(key, typeof({keyType.ToDisplayString()}), CultureInfo.InvariantCulture)";
                    sb.AppendLine($"{indent}        var convertedKey = {keyConversion};");
                    GenerateFromJsonValue(sb, valueType, $"{propertyName}Dictionary", indent + "        ", readerAccessor, "convertedKey");
                    sb.AppendLine($"{indent}        {readerAccessor}.SkipWhitespace();");
                    sb.AppendLine($"{indent}        if ({readerAccessor}.Current == (byte)',')");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            {readerAccessor}.ReadNext();");
                    sb.AppendLine($"{indent}            {readerAccessor}.SkipWhitespace();"); // Skip whitespace after comma
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}        else if ({readerAccessor}.Current == (byte)'}}')");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            {readerAccessor}.ReadEndObject();");
                    sb.AppendLine($"{indent}            break;");
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}        else");
                    sb.AppendLine($"{indent}        {{");
                    sb.AppendLine($"{indent}            throw new FluxJson.Core.JsonException(\"Expected ',' or '}}' at position \" + {readerAccessor}.Position);");
                    sb.AppendLine($"{indent}        }}");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}}}"); // Close the else block for non-empty dictionary
                    sb.AppendLine($"{indent}instance.{propertyName} = {propertyName}Dictionary;");
                }
                else
                {
                    sb.AppendLine($"{indent}// TODO: Implement deserialization for generic type {type.ToDisplayString()}");
                    sb.AppendLine($"{indent}{readerAccessor}.SkipValue();");
                }
            }
            else
            {
                sb.AppendLine($"{indent}// TODO: Implement deserialization for type {type.ToDisplayString()}");
                sb.AppendLine($"{indent}{readerAccessor}.SkipValue();");
            }

            if (isNullableType || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.Annotated))
            {
                sb.AppendLine($"{indent}}}");
            }
        }

        private static void GenerateFromJsonValue(StringBuilder sb, ITypeSymbol type, string collectionName, string indent, string readerAccessor, string? dictionaryKey = null)
        {
            bool isNullableType = type.NullableAnnotation == NullableAnnotation.Annotated;
            ITypeSymbol underlyingType = isNullableType && type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? namedType.TypeArguments[0] : type;

            string assignmentTarget;
            if (dictionaryKey != null)
            {
                assignmentTarget = $"{collectionName}[{dictionaryKey}] = ";
            }
            else
            {
                assignmentTarget = $"{collectionName}.Add(";
            }

            string currentIndent = indent;

            if (isNullableType)
            {
                sb.AppendLine($"{currentIndent}if ({readerAccessor}.TryReadNull())");
                sb.AppendLine($"{currentIndent}{{");
                if (dictionaryKey != null)
                {
                    sb.AppendLine($"{currentIndent}    {collectionName}[{dictionaryKey}] = default;");
                }
                else
                {
                    sb.AppendLine($"{currentIndent}    {collectionName}.Add(default);");
                }
                sb.AppendLine($"{currentIndent}}}");
                sb.AppendLine($"{currentIndent}else");
                sb.AppendLine($"{currentIndent}{{");
                currentIndent += "    ";
            }
            else if (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.Annotated)
            {
                sb.AppendLine($"{currentIndent}if ({readerAccessor}.TryReadNull())");
                sb.AppendLine($"{currentIndent}{{");
                // FIXED: Properly escape the } character in the interpolated string
                sb.AppendLine($"{currentIndent}    throw new FluxJson.Core.JsonException(\"Unexpected null value for non-nullable type '{underlyingType.ToDisplayString()}' at position \" + reader.Position);");
                sb.AppendLine($"{currentIndent}}}");
                sb.AppendLine($"{currentIndent}else");
                sb.AppendLine($"{currentIndent}{{");
                currentIndent += "    ";
            }

            if (underlyingType.SpecialType == SpecialType.System_String)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadString()!;");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadString()!);");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Int32)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadInt32();");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadInt32());");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Boolean)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadBoolean();");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadBoolean());");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Double)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadDouble();");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadDouble());");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Single)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadSingle();");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadSingle());");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Decimal)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadDecimal();");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{readerAccessor}.ReadDecimal());");
            }
            else if (underlyingType.SpecialType == SpecialType.System_DateTime)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}DateTime.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture);");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}DateTime.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture));");
            }
            else if (underlyingType.ToDisplayString() == "System.DateTimeOffset")
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}DateTimeOffset.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture);");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}DateTimeOffset.Parse({readerAccessor}.ReadString()!, CultureInfo.InvariantCulture));");
            }
            else if (underlyingType.ToDisplayString() == "System.Guid")
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}Guid.Parse({readerAccessor}.ReadString()!);");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}Guid.Parse({readerAccessor}.ReadString()!));");
            }
            else if (underlyingType.TypeKind == TypeKind.Enum)
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}Enum.Parse<{underlyingType.ToDisplayString()}>({readerAccessor}.ReadString()!);");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}Enum.Parse<{underlyingType.ToDisplayString()}>({readerAccessor}.ReadString()!));");
            }
            else if (underlyingType.TypeKind == TypeKind.Class && underlyingType is INamedTypeSymbol namedClassType && namedClassType.AllInterfaces.Any(i => i.IsGenericType && i.ConstructedFrom.ToDisplayString().StartsWith("FluxJson.Core.IJsonSerializable<")))
            {
                if (dictionaryKey != null)
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{underlyingType.Name}.FromJson({readerAccessor})!;");
                else
                    sb.AppendLine($"{currentIndent}{assignmentTarget}{underlyingType.Name}.FromJson({readerAccessor}));");
                // FromJsonSerializable already handles advancing the reader past the object
            }
            else
            {
                sb.AppendLine($"{currentIndent}// TODO: Implement deserialization for type {type.ToDisplayString()}");
                sb.AppendLine($"{currentIndent}{readerAccessor}.SkipValue();");
                sb.AppendLine($"{currentIndent}{readerAccessor}.ReadNext();"); // Advance reader past the skipped value
            }

            if (isNullableType || (type.IsReferenceType && type.NullableAnnotation != NullableAnnotation.Annotated))
            {
                sb.AppendLine($"{indent}}}");
            }
        }
    }
}
