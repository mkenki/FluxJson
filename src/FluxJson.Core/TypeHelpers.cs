// src/FluxJson.Core/TypeHelpers.cs
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace FluxJson.Core;

internal static class TypeHelpers
{
    private static readonly ConcurrentDictionary<Type, Type> _collectionElementTypeCache = new();
    private static readonly ConcurrentDictionary<Type, Func<int, IList>> _listConstructorCache = new();
    private static readonly ConcurrentDictionary<Type, Func<IDictionary>> _dictionaryConstructorCache = new();

    public static bool IsCollectionType(Type type)
    {
        return type != typeof(string) &&
               (type.IsArray ||
                typeof(IEnumerable).IsAssignableFrom(type));
    }

    public static bool IsDictionaryType(Type type)
    {
        return typeof(IDictionary).IsAssignableFrom(type) ||
               (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                                       type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>)));
    }

    public static Type GetCollectionElementType(Type type)
    {
        return _collectionElementTypeCache.GetOrAdd(type, t =>
        {
            if (t.IsArray)
                return t.GetElementType()!;

            if (t.IsGenericType)
            {
                var genericArgs = t.GetGenericArguments();
                if (genericArgs.Length == 1)
                    return genericArgs[0];
            }

            // Try to find IEnumerable<T>
            var enumerableInterface = t.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
                return enumerableInterface.GetGenericArguments()[0];

            return typeof(object);
        });
    }

    public static bool IsNullableType(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static Type GetNullableUnderlyingType(Type type)
    {
        return IsNullableType(type) ? type.GetGenericArguments()[0] : type;
    }

    public static IList CreateCollectionInstance(Type targetType, Type elementType, int capacity)
    {
        if (targetType.IsArray)
        {
            return Array.CreateInstance(elementType, capacity);
        }

        return _listConstructorCache.GetOrAdd(targetType, t =>
        {
            if (t.IsGenericType)
            {
                var genericDef = t.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>) || genericDef == typeof(IList<>) || genericDef == typeof(ICollection<>))
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var constructor = listType.GetConstructor(new[] { typeof(int) });
                    if (constructor != null)
                    {
                        return cap => (IList)constructor.Invoke(new object[] { cap });
                    }
                }
                else if (genericDef == typeof(HashSet<>))
                {
                    var hashSetType = typeof(HashSet<>).MakeGenericType(elementType);
                    var constructor = hashSetType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return cap => (IList)Activator.CreateInstance(hashSetType)!; // HashSet doesn't take capacity in constructor
                    }
                }
                else if (genericDef == typeof(Queue<>))
                {
                    var queueType = typeof(Queue<>).MakeGenericType(elementType);
                    var constructor = queueType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return cap => (IList)Activator.CreateInstance(queueType)!;
                    }
                }
                else if (genericDef == typeof(Stack<>))
                {
                    var stackType = typeof(Stack<>).MakeGenericType(elementType);
                    var constructor = stackType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return cap => (IList)Activator.CreateInstance(stackType)!;
                    }
                }
            }
            // Fallback to List<object> if specific type cannot be instantiated
            return cap => (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType), cap)!;
        }).Invoke(capacity);
    }

    public static IDictionary CreateDictionaryInstance(Type targetType, Type keyType, Type valueType)
    {
        return _dictionaryConstructorCache.GetOrAdd(targetType, t =>
        {
            if (t.IsGenericType)
            {
                var genericDef = t.GetGenericTypeDefinition();
                if (genericDef == typeof(Dictionary<,>) || genericDef == typeof(IDictionary<,>) || genericDef == typeof(IReadOnlyDictionary<,>))
                {
                    var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                    var constructor = dictType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return () => (IDictionary)constructor.Invoke(null)!;
                    }
                }
                else if (genericDef == typeof(ConcurrentDictionary<,>))
                {
                    var dictType = typeof(ConcurrentDictionary<,>).MakeGenericType(keyType, valueType);
                    var constructor = dictType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return () => (IDictionary)constructor.Invoke(null)!;
                    }
                }
                else if (genericDef == typeof(SortedList<,>))
                {
                    var dictType = typeof(SortedList<,>).MakeGenericType(keyType, valueType);
                    var constructor = dictType.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        return () => (IDictionary)constructor.Invoke(null)!;
                    }
                }
            }
            // Fallback to Dictionary<object, object>
            return () => (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType))!;
        }).Invoke();
    }
}
