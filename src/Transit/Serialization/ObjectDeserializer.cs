using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Transit.Impl;

namespace Transit.Serialization;

/// <summary>
/// Provides high-performance deserialization for POCOs using compiled expression trees.
/// </summary>
public static class ObjectDeserializer
{
    private static readonly ConcurrentDictionary<Type, Func<IDictionary, object>> _deserializerCache = new();

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
    /// </summary>
    public static object? MapValue(object? value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsAssignableFrom(value.GetType())) return value;

        if (value is IDictionary dict && !IsBuiltInType(targetType))
        {
            return Deserialize(dict, targetType);
        }

        if (value is IList list && typeof(ITuple).IsAssignableFrom(targetType))
        {
            return MapValueTuple(list, targetType);
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value.ToString()!);
        }

        // Arrays: T[]
        if (targetType.IsArray && value is IList arrayList)
        {
            var elemType = targetType.GetElementType()!;
            var arr = Array.CreateInstance(elemType, arrayList.Count);
            for (int i = 0; i < arrayList.Count; i++)
            {
                arr.SetValue(MapValue(arrayList[i], elemType), i);
            }
            return arr;
        }

        // Generic list-like collections from IList source
        if (value is IList sourceList && targetType.IsGenericType)
        {
            var genDef = targetType.GetGenericTypeDefinition();
            var elemType = targetType.GetGenericArguments()[0];

            // List<T>, IList<T>, ICollection<T>, IEnumerable<T>, IReadOnlyList<T>, IReadOnlyCollection<T>
            if (genDef == typeof(List<>) || genDef == typeof(IList<>) ||
                genDef == typeof(ICollection<>) || genDef == typeof(IEnumerable<>) ||
                genDef == typeof(IReadOnlyList<>) || genDef == typeof(IReadOnlyCollection<>))
            {
                var listType = typeof(List<>).MakeGenericType(elemType);
                var result = (IList)Activator.CreateInstance(listType, sourceList.Count)!;
                for (int i = 0; i < sourceList.Count; i++)
                {
                    result.Add(MapValue(sourceList[i], elemType));
                }
                return result;
            }
        }

        // HashSet<T>, ISet<T>, IReadOnlySet<T> from any IEnumerable source (incl. HashSet<object>)
        if (value is IEnumerable sourceEnumerable && targetType.IsGenericType)
        {
            var genDef = targetType.GetGenericTypeDefinition();
            if (genDef == typeof(HashSet<>) || genDef == typeof(ISet<>)
#if NET5_0_OR_GREATER
                || genDef == typeof(IReadOnlySet<>)
#endif
                )
            {
                var elemType = targetType.GetGenericArguments()[0];
                var setType = typeof(HashSet<>).MakeGenericType(elemType);
                var set = Activator.CreateInstance(setType)!;
                var addMethod = setType.GetMethod("Add")!;
                foreach (var item in sourceEnumerable)
                {
                    addMethod.Invoke(set, [MapValue(item, elemType)]);
                }
                return set;
            }
        }

        // Generic dictionaries from IDictionary source
        if (value is IDictionary sourceDict && targetType.IsGenericType)
        {
            var genDef = targetType.GetGenericTypeDefinition();
            if (genDef == typeof(Dictionary<,>) || genDef == typeof(IDictionary<,>) ||
                genDef == typeof(IReadOnlyDictionary<,>))
            {
                var args = targetType.GetGenericArguments();
                var keyType = args[0];
                var valType = args[1];
                var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valType);
                var resultDict = (IDictionary)Activator.CreateInstance(dictType)!;
                foreach (DictionaryEntry entry in sourceDict)
                {
                    resultDict.Add(MapValue(entry.Key, keyType)!, MapValue(entry.Value, valType)!);
                }
                return resultDict;
            }
        }

        if (targetType == typeof(int)) return (int)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(long)) return Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(short)) return (short)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(byte)) return (byte)Util.NumberToPrimitiveLong(value);
        if (targetType == typeof(float)) return Convert.ToSingle(value);
        if (targetType == typeof(double)) return Convert.ToDouble(value);
        if (targetType == typeof(decimal)) return Convert.ToDecimal(value);

        // String conversion (handles Keyword → string, etc.)
        if (targetType == typeof(string)) return value.ToString();

        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch (InvalidCastException)
        {
            return value;
        }
    }

    private static object MapValueTuple(IList list, Type targetType)
    {
        // ValueTuples have fields Item1, Item2...
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.Name.StartsWith("Item"))
            .OrderBy(f => f.Name.Length)
            .ThenBy(f => f.Name)
            .ToArray();

        object instance = Activator.CreateInstance(targetType)!;
        for (int i = 0; i < Math.Min(list.Count, fields.Length); i++)
        {
            fields[i].SetValue(instance, MapValue(list[i], fields[i].FieldType));
        }
        return instance;
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
