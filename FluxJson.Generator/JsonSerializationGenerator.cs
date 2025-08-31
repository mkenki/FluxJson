using System.Linq;
using System.Text;
using FluxJson; // Assuming FluxJson.Core is needed for JsonWriter
using Microsoft.CodeAnalysis;

namespace FluxJson.Generator
{
    internal static class JsonSerializationGenerator
    {
        public static void GenerateToJsonMethod(StringBuilder sb, INamedTypeSymbol classSymbol)
        {
            sb.AppendLine("        public void ToJson(ref FluxJson.Core.JsonWriter writer)");
            sb.AppendLine("        {");
            sb.AppendLine("            writer.WriteStartObject();");

            var properties = classSymbol.GetMembers().OfType<IPropertySymbol>()
                                       .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public && p.GetMethod != null && p.GetMethod.DeclaredAccessibility == Accessibility.Public)
                                       .ToList();

            bool firstProperty = true;
            foreach (var property in properties)
            {
                if (!firstProperty)
                {
                    sb.AppendLine("            writer.WriteSeparator();");
                }
                firstProperty = false;

                string propertyName = property.Name;
                ITypeSymbol propertyType = property.Type;
                bool isNullable = propertyType.NullableAnnotation == NullableAnnotation.Annotated;
                ITypeSymbol underlyingType = isNullable && propertyType is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? namedType.TypeArguments[0] : propertyType;

                sb.AppendLine($"            writer.WritePropertyName(\"{propertyName}\");");

                if (isNullable)
                {
                    sb.AppendLine($"            if (this.{propertyName} == null)");
                    sb.AppendLine("            {");
                    sb.AppendLine("                writer.WriteNull();");
                    sb.AppendLine("            }");
                    sb.AppendLine("            else");
                    sb.AppendLine("            {");
                }

                GenerateToJsonPropertyValue(sb, underlyingType, propertyName, isNullable);

                if (isNullable)
                {
                    sb.AppendLine("            }");
                }
            }

            sb.AppendLine("            writer.WriteEndObject();");
            sb.AppendLine("        }");
        }

        private static void GenerateToJsonPropertyValue(StringBuilder sb, ITypeSymbol type, string propertyName, bool isNullable)
        {
            string indent = isNullable ? "                " : "            ";
            string valueAccessor = $"this.{propertyName}";

            if (type.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor});");
            }
            else if (type.SpecialType == SpecialType.System_Int32 ||
                     type.SpecialType == SpecialType.System_Boolean ||
                     type.SpecialType == SpecialType.System_Double ||
                     type.SpecialType == SpecialType.System_Single ||
                     type.SpecialType == SpecialType.System_Decimal)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor});");
            }
            else if (type.SpecialType == SpecialType.System_DateTime || type.ToDisplayString() == "System.DateTimeOffset")
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString(\"O\")); // ISO 8601 format");
            }
            else if (type.ToDisplayString() == "System.Guid")
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString());");
            }
            else if (type.TypeKind == TypeKind.Enum)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString());");
            }
            else if (type.TypeKind == TypeKind.Class && type is INamedTypeSymbol namedClassType && namedClassType.AllInterfaces.Any(i => i.IsGenericType && i.ConstructedFrom.ToDisplayString().StartsWith("FluxJson.Core.IJsonSerializable<")))
            {
                sb.AppendLine($"{indent}{valueAccessor}.ToJson(ref writer);");
            }
            else if (type is IArrayTypeSymbol arrayType)
            {
                ITypeSymbol elementType = arrayType.ElementType;
                sb.AppendLine($"{indent}writer.WriteStartArray();");
                sb.AppendLine($"{indent}bool firstItem = true;");
                sb.AppendLine($"{indent}foreach (var item in {valueAccessor})");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    if (!firstItem)");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}        writer.WriteSeparator();");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine($"{indent}    firstItem = false;");
                GenerateToJsonValue(sb, elementType, "item", indent + "    ");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}writer.WriteEndArray();");
            }
            else if (type is INamedTypeSymbol collectionType && collectionType.IsGenericType)
            {
                if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.List<T>" ||
                    collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
                {
                    ITypeSymbol itemType = collectionType.TypeArguments[0];
                    sb.AppendLine($"{indent}writer.WriteStartArray();");
                    sb.AppendLine($"{indent}bool firstItem = true;");
                    sb.AppendLine($"{indent}foreach (var item in {valueAccessor})");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    if (!firstItem)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        writer.WriteSeparator();");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    firstItem = false;");
                    GenerateToJsonValue(sb, itemType, "item", indent + "    ");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndArray();");
                }
                else if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
                {
                    ITypeSymbol keyType = collectionType.TypeArguments[0];
                    ITypeSymbol valueType = collectionType.TypeArguments[1];
                    sb.AppendLine($"{indent}writer.WriteStartObject();");
                    sb.AppendLine($"{indent}bool firstKvp = true;");
                    sb.AppendLine($"{indent}foreach (var kvp in {valueAccessor})");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    if (!firstKvp)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        writer.WriteSeparator();");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    firstKvp = false;");
                    sb.AppendLine($"{indent}    writer.WritePropertyName(kvp.Key.ToString());");
                    GenerateToJsonValue(sb, valueType, "kvp.Value", indent + "    ");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndObject();");
                }
                else
                {
                    sb.AppendLine($"{indent}// TODO: Implement serialization for generic type {type.ToDisplayString()}");
                    sb.AppendLine($"{indent}writer.WriteNull(); // Placeholder for unsupported type");
                }
            }
            else
            {
                sb.AppendLine($"{indent}// TODO: Implement serialization for type {type.ToDisplayString()}");
                sb.AppendLine($"{indent}writer.WriteNull(); // Placeholder for unsupported type");
            }
        }

        private static void GenerateToJsonValue(StringBuilder sb, ITypeSymbol type, string valueAccessor, string indent)
        {
            bool isNullable = type.NullableAnnotation == NullableAnnotation.Annotated;
            ITypeSymbol underlyingType = isNullable && type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T ? namedType.TypeArguments[0] : type;

            if (isNullable)
            {
                sb.AppendLine($"{indent}if ({valueAccessor} == null)");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    writer.WriteNull();");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}else");
                sb.AppendLine($"{indent}    {{");
                indent += "    ";
            }

            if (underlyingType.SpecialType == SpecialType.System_String)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor});");
            }
            else if (underlyingType.SpecialType == SpecialType.System_Int32 ||
                     underlyingType.SpecialType == SpecialType.System_Boolean ||
                     underlyingType.SpecialType == SpecialType.System_Double ||
                     underlyingType.SpecialType == SpecialType.System_Single ||
                     underlyingType.SpecialType == SpecialType.System_Decimal)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor});");
            }
            else if (underlyingType.SpecialType == SpecialType.System_DateTime || underlyingType.ToDisplayString() == "System.DateTimeOffset")
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString(\"O\"));");
            }
            else if (underlyingType.ToDisplayString() == "System.Guid")
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString());");
            }
            else if (underlyingType.TypeKind == TypeKind.Enum)
            {
                sb.AppendLine($"{indent}writer.WriteValue({valueAccessor}.ToString());");
            }
            else if (underlyingType.TypeKind == TypeKind.Class && underlyingType is INamedTypeSymbol namedClassType && namedClassType.AllInterfaces.Any(i => i.IsGenericType && i.ConstructedFrom.ToDisplayString().StartsWith("FluxJson.Core.IJsonSerializable<")))
            {
                sb.AppendLine($"{indent}{valueAccessor}.ToJson(ref writer);");
            }
            else if (underlyingType is IArrayTypeSymbol arrayType)
            {
                ITypeSymbol elementType = arrayType.ElementType;
                sb.AppendLine($"{indent}writer.WriteStartArray();");
                sb.AppendLine($"{indent}bool firstItem = true;");
                sb.AppendLine($"{indent}foreach (var item in {valueAccessor})");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    if (!firstItem)");
                sb.AppendLine($"{indent}    {{");
                sb.AppendLine($"{indent}        writer.WriteSeparator();");
                sb.AppendLine($"{indent}    }}");
                sb.AppendLine($"{indent}    firstItem = false;");
                GenerateToJsonValue(sb, elementType, "item", indent + "    ");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}writer.WriteEndArray();");
            }
            else if (underlyingType is INamedTypeSymbol collectionType && collectionType.IsGenericType)
            {
                if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.List<T>" ||
                    collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
                {
                    ITypeSymbol itemType = collectionType.TypeArguments[0];
                    sb.AppendLine($"{indent}writer.WriteStartArray();");
                    sb.AppendLine($"{indent}bool firstItem = true;");
                    sb.AppendLine($"{indent}foreach (var item in {valueAccessor})");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    if (!firstItem)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        writer.WriteSeparator();");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    firstItem = false;");
                    GenerateToJsonValue(sb, itemType, "item", indent + "    ");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndArray();");
                }
                else if (collectionType.ConstructedFrom.ToDisplayString() == "System.Collections.Generic.Dictionary<TKey, TValue>")
                {
                    ITypeSymbol keyType = collectionType.TypeArguments[0];
                    ITypeSymbol valueType = collectionType.TypeArguments[1];
                    sb.AppendLine($"{indent}writer.WriteStartObject();");
                    sb.AppendLine($"{indent}bool firstKvp = true;");
                    sb.AppendLine($"{indent}foreach (var kvp in {valueAccessor})");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    if (!firstKvp)");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        writer.WriteSeparator();");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    firstKvp = false;");
                    sb.AppendLine($"{indent}    writer.WritePropertyName(kvp.Key.ToString());");
                    GenerateToJsonValue(sb, valueType, "kvp.Value", indent + "    ");
                    sb.AppendLine($"{indent}}}");
                    sb.AppendLine($"{indent}writer.WriteEndObject();");
                }
                else
                {
                    sb.AppendLine($"{indent}// TODO: Implement serialization for generic type {type.ToDisplayString()}");
                    sb.AppendLine($"{indent}writer.WriteNull();");
                }
            }
            else
            {
                sb.AppendLine($"{indent}// TODO: Implement serialization for type {type.ToDisplayString()}");
                sb.AppendLine($"{indent}writer.WriteNull();");
            }

            if (isNullable)
            {
                sb.AppendLine($"{indent.Substring(4)}}}");
            }
        }
    }
}
