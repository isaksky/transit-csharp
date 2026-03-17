using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Transit.Impl;

namespace Transit.Serialization;

/// <summary>
/// Provides high-performance deserialization for POCOs using compiled expression trees
/// and cached per-type converters.
/// </summary>
public static class ObjectDeserializer
{
    private static readonly ConcurrentDictionary<Type, Func<IDictionary, object>> _deserializerCache = new();
    private static readonly ConcurrentDictionary<Type, Func<object, object?>> _converterCache = new();

    /// <summary>
    /// Deserializes a dictionary into an object of the specified type.
    /// </summary>
    public static T Deserialize<T>(IDictionary dict) => (T)Deserialize(dict, typeof(T));

    /// <summary>
    /// Deserializes a dictionary into an object of the specified type.
    /// </summary>
    public static object Deserialize(IDictionary dict, Type type)
    {
        var deserializer = _deserializerCache.GetOrAdd(type, CreateDeserializer);
        return deserializer(dict);
    }

    /// <summary>
    /// Maps a value to the target type, handling recursive POCO deserialization and conversions.
    /// Uses cached per-type converters to avoid repeated reflection.
    /// </summary>
    public static object? MapValue(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsAssignableFrom(value.GetType())) return value;

        var converter = _converterCache.GetOrAdd(targetType, CreateConverter);
        return converter(value);
    }

    /// <summary>
    /// Creates a cached converter function for the given target type.
    /// All reflection (GetGenericTypeDefinition, MakeGenericType, GetMethod, etc.)
    /// happens once here; subsequent calls just invoke the delegate.
    /// </summary>
    private static Func<object, object?> CreateConverter(Type targetType)
    {
        // Enum
        if (targetType.IsEnum)
        {
            return value => Enum.Parse(targetType, value.ToString()!);
        }

        // Arrays: T[]
        if (targetType.IsArray)
        {
            var elemType = targetType.GetElementType()!;
            return value =>
            {
                if (value is IList list)
                {
                    var arr = Array.CreateInstance(elemType, list.Count);
                    for (int i = 0; i < list.Count; i++)
                    {
                        arr.SetValue(MapValue(list[i], elemType), i);
                    }
                    return arr;
                }
                return value;
            };
        }

        // ValueTuples
        if (typeof(ITuple).IsAssignableFrom(targetType))
        {
            var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.Name.StartsWith("Item"))
                .OrderBy(f => f.Name.Length)
                .ThenBy(f => f.Name)
                .ToArray();
            var fieldTypes = fields.Select(f => f.FieldType).ToArray();

            // Find the constructor that takes all the fields
            var ctor = targetType.GetConstructor(fieldTypes);
            
            if (ctor != null)
            {
                // Compile factory: args => new TupleType((T1)args[0], (T2)args[1], ...)
                var argsParam = Expression.Parameter(typeof(object[]), "args");
                var ctorArgs = new Expression[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    var arrayAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    ctorArgs[i] = Expression.Convert(arrayAccess, fieldTypes[i]);
                }
                var createTuple = Expression.Lambda<Func<object[], object>>(
                    Expression.Convert(Expression.New(ctor, ctorArgs), typeof(object)),
                    argsParam).Compile();

                return value =>
                {
                    if (value is IList list)
                    {
                        int count = Math.Min(list.Count, fields.Length);
                        var args = new object[fields.Length]; // Default initialized to null/0
                        for (int i = 0; i < count; i++)
                        {
                            args[i] = MapValue(list[i], fieldTypes[i])!;
                        }
                        // For any missing items, we need default value to satisfy ctor
                        for (int i = count; i < fields.Length; i++)
                        {
                            args[i] = fieldTypes[i].IsValueType ? Activator.CreateInstance(fieldTypes[i])! : null!;
                        }
                        
                        return createTuple(args);
                    }
                    return value;
                };
            }
        }

        // Generic single-element collections
        if (targetType.IsGenericType)
        {
            var genDef = targetType.GetGenericTypeDefinition();
            var genArgs = targetType.GetGenericArguments();

            // List<T>, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
            if (genDef == typeof(List<>) || genDef == typeof(IList<>) ||
                genDef == typeof(ICollection<>) || genDef == typeof(IEnumerable<>) ||
                genDef == typeof(IReadOnlyList<>) || genDef == typeof(IReadOnlyCollection<>))
            {
                var elemType = genArgs[0];
                var listType = typeof(List<>).MakeGenericType(elemType);
                var ctor = listType.GetConstructor([typeof(int)])!;
                
                // Compile list creator: count => new List<T>(count)
                var countParam = Expression.Parameter(typeof(int), "count");
                var createList = Expression.Lambda<Func<int, IList>>(
                    Expression.Convert(Expression.New(ctor, countParam), typeof(IList)),
                    countParam).Compile();

                var addMethod = listType.GetMethod("Add")!;
                // Build a compiled add delegate: (list, item) => ((List<T>)list).Add((T)item)
                var listParam = Expression.Parameter(typeof(object), "list");
                var itemParam = Expression.Parameter(typeof(object), "item");
                var addCall = Expression.Call(
                    Expression.Convert(listParam, listType),
                    addMethod,
                    Expression.Convert(itemParam, elemType));
                var addDelegate = Expression.Lambda<Action<object, object?>>(addCall, listParam, itemParam).Compile();

                return value =>
                {
                    if (value is IList sourceList)
                    {
                        var result = createList(sourceList.Count);
                        for (int i = 0; i < sourceList.Count; i++)
                        {
                            addDelegate(result, MapValue(sourceList[i], elemType));
                        }
                        return result;
                    }
                    return value;
                };
            }

            // HashSet<T>, ISet<T>, IReadOnlySet<T>
            if (genDef == typeof(HashSet<>) || genDef == typeof(ISet<>)
#if NET5_0_OR_GREATER
                || genDef == typeof(IReadOnlySet<>)
#endif
                )
            {
                var elemType = genArgs[0];
                var setType = typeof(HashSet<>).MakeGenericType(elemType);
                
                // Compile set creator: () => new HashSet<T>()
                var createSet = Expression.Lambda<Func<object>>(Expression.New(setType)).Compile();

                var setAddMethod = setType.GetMethod("Add")!;
                // Build a compiled add delegate
                var setParam = Expression.Parameter(typeof(object), "set");
                var itemParam = Expression.Parameter(typeof(object), "item");
                var addCall = Expression.Call(
                    Expression.Convert(setParam, setType),
                    setAddMethod,
                    Expression.Convert(itemParam, elemType));
                var addDelegate = Expression.Lambda<Action<object, object?>>(addCall, setParam, itemParam).Compile();

                return value =>
                {
                    if (value is IEnumerable source)
                    {
                        var set = createSet();
                        foreach (var item in source)
                        {
                            addDelegate(set, MapValue(item, elemType));
                        }
                        return set;
                    }
                    return value;
                };
            }

            // Dictionary<K,V>, IDictionary<K,V>, IReadOnlyDictionary<K,V>
            if (genDef == typeof(Dictionary<,>) || genDef == typeof(IDictionary<,>) ||
                genDef == typeof(IReadOnlyDictionary<,>))
            {
                var keyType = genArgs[0];
                var valType = genArgs[1];
                var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valType);
                
                // Compile dict creator: () => new Dictionary<K,V>()
                var createDict = Expression.Lambda<Func<IDictionary>>(
                    Expression.Convert(Expression.New(dictType), typeof(IDictionary))).Compile();

                var dictAddMethod = dictType.GetMethod("Add", [keyType, valType])!;
                // Build compiled add delegate: (dict, key, val) => ((Dict<K,V>)dict).Add((K)key, (V)val)
                var dictParam = Expression.Parameter(typeof(object), "dict");
                var keyParam = Expression.Parameter(typeof(object), "key");
                var valParam = Expression.Parameter(typeof(object), "val");
                var addExpr = Expression.Call(
                    Expression.Convert(dictParam, dictType),
                    dictAddMethod,
                    Expression.Convert(keyParam, keyType),
                    Expression.Convert(valParam, valType));
                var addDelegate = Expression.Lambda<Action<object, object, object?>>(addExpr, dictParam, keyParam, valParam).Compile();

                return value =>
                {
                    if (value is IDictionary sourceDict)
                    {
                        var result = createDict();
                        foreach (DictionaryEntry entry in sourceDict)
                        {
                            addDelegate(result, MapValue(entry.Key, keyType)!, MapValue(entry.Value, valType)!);
                        }
                        return result;
                    }
                    return value;
                };
            }
        }

        // POCO from IDictionary (non-built-in complex types)
        if (!IsBuiltInType(targetType))
        {
            return value =>
            {
                if (value is IDictionary dict)
                    return Deserialize(dict, targetType);
                return value;
            };
        }

        // Numeric primitives — direct, no reflection needed per call
        if (targetType == typeof(int)) return value => (int)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(long)) return value => Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(short)) return value => (short)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(byte)) return value => (byte)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(float)) return value => Convert.ToSingle(value);
        if (targetType == typeof(double)) return value => Convert.ToDouble(value);
        if (targetType == typeof(decimal)) return value => Convert.ToDecimal(value);

        // String
        if (targetType == typeof(string)) return value => value.ToString();

        // Fallback
        return value =>
        {
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch (InvalidCastException)
            {
                return value;
            }
        };
    }

    private static bool IsBuiltInType(Type type)
    {
        return type.IsPrimitive || 
               type == typeof(string) || 
               type == typeof(decimal) || 
               type == typeof(DateTime) || 
               type == typeof(Guid) || 
               type == typeof(Uri) ||
               type == typeof(TimeSpan) ||
               type == typeof(DateTimeOffset) ||
               typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static Func<IDictionary, object> CreateDeserializer(Type type)
    {
        var dictParam = Expression.Parameter(typeof(IDictionary), "dict");
        var instanceVar = Expression.Variable(type, "instance");
        
        var body = new List<Expression>();
        
        // instance = new T();
        body.Add(Expression.Assign(instanceVar, Expression.New(type)));

        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);
        
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        var getItemMethod = typeof(IDictionary).GetMethod("get_Item");
        var containsMethod = typeof(IDictionary).GetMethod("Contains");

        foreach (var prop in props)
        {
            body.Add(CreateBindingExpression(dictParam, instanceVar, prop.Name, prop.PropertyType, value => Expression.Assign(Expression.Property(instanceVar, prop), value)));
        }

        foreach (var field in fields)
        {
            body.Add(CreateBindingExpression(dictParam, instanceVar, field.Name, field.FieldType, value => Expression.Assign(Expression.Field(instanceVar, field), value)));
        }

        body.Add(Expression.Convert(instanceVar, typeof(object)));

        var block = Expression.Block(new[] { instanceVar }, body);
        return Expression.Lambda<Func<IDictionary, object>>(block, dictParam).Compile();

        Expression CreateBindingExpression(ParameterExpression dict, ParameterExpression instance, string name, Type memberType, Func<Expression, Expression> assigner)
        {
            var stringKey = Expression.Constant(name);
            var keywordKey = Expression.Call(typeof(TransitFactory).GetMethod("Keyword", new[] { typeof(object) })!, stringKey);

            var valueVar = Expression.Variable(typeof(object), "val");
            
            var tryString = Expression.IfThen(
                Expression.Call(dict, containsMethod!, stringKey),
                Expression.Assign(valueVar, Expression.Call(dict, getItemMethod!, stringKey))
            );

            var tryKeyword = Expression.IfThen(
                Expression.Call(dict, containsMethod!, keywordKey),
                Expression.Assign(valueVar, Expression.Call(dict, getItemMethod!, keywordKey))
            );

            var mapValueMethod = typeof(ObjectDeserializer).GetMethod("MapValue")!;
            var assignValue = Expression.IfThen(
                Expression.NotEqual(valueVar, Expression.Constant(null)),
                assigner(Expression.Convert(Expression.Call(mapValueMethod, valueVar, Expression.Constant(memberType)), memberType))
            );

            return Expression.Block(new[] { valueVar },
                Expression.Assign(valueVar, Expression.Constant(null)),
                tryString,
                tryKeyword,
                assignValue
            );
        }
    }
}
