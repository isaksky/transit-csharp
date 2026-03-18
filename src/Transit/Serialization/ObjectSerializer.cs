using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Transit.Net.Serialization;

/// <summary>
/// Provides high-performance serialization for POCOs using compiled expression trees.
/// </summary>
public static class ObjectSerializer
{
    private static readonly ConcurrentDictionary<(Type, bool), IWriteHandler> _handlerCache = new();

    /// <summary>
    /// Gets a write handler for the specified type.
    /// </summary>
    public static IWriteHandler GetHandler(Type type, bool useKeywordKeys)
    {
        return _handlerCache.GetOrAdd((type, useKeywordKeys), t => CreateHandler(t.Item1, t.Item2));
    }

    private static IWriteHandler CreateHandler(Type type, bool useKeywordKeys)
    {
        var info = new CompiledTypeSerializationInfo(type, useKeywordKeys);
        return new PocoWriteHandler(info);
    }

    private class PocoWriteHandler : IWriteHandler
    {
        private readonly CompiledTypeSerializationInfo _info;

        public PocoWriteHandler(CompiledTypeSerializationInfo info)
        {
            _info = info;
        }

        public string Tag(object obj) => "map";

        public object Representation(object obj) => new PocoDictionary(obj, _info);

        public string? StringRepresentation(object obj) => null;

        public IWriteHandler? GetVerboseHandler() => null;
    }

    private class CompiledTypeSerializationInfo
    {
        public object[] Keys { get; }
        public Func<object, object[]> Accessor { get; }

        public CompiledTypeSerializationInfo(Type type, bool useKeywordKeys)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();
            
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .ToList();

            var keys = new List<object>();
            var values = new List<Expression>();
            var instanceParam = Expression.Parameter(typeof(object), "obj");
            var convertedInstance = Expression.Convert(instanceParam, type);

            foreach (var prop in props)
            {
                keys.Add(useKeywordKeys ? TransitFactory.Keyword(prop.Name) : prop.Name);
                values.Add(Expression.Convert(Expression.Property(convertedInstance, prop), typeof(object)));
            }

            foreach (var field in fields)
            {
                keys.Add(useKeywordKeys ? TransitFactory.Keyword(field.Name) : field.Name);
                values.Add(Expression.Convert(Expression.Field(convertedInstance, field), typeof(object)));
            }

            Keys = keys.ToArray();
            var arrayInit = Expression.NewArrayInit(typeof(object), values);
            Accessor = Expression.Lambda<Func<object, object[]>>(arrayInit, instanceParam).Compile();
        }
    }

    private class PocoDictionary : IDictionary
    {
        private readonly object _instance;
        private readonly CompiledTypeSerializationInfo _info;
        private object[]? _values;

        public PocoDictionary(object instance, CompiledTypeSerializationInfo info)
        {
            _instance = instance;
            _info = info;
        }

        private object[] Values => _values ??= _info.Accessor(_instance);

        public int Count => _info.Keys.Length;

        public bool IsSynchronized => false;

        public object SyncRoot => _instance;

        public bool IsFixedSize => true;

        public bool IsReadOnly => true;

        public object? this[object key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public ICollection Keys => _info.Keys;

        public ICollection ValuesCollection => Values;

        ICollection IDictionary.Values => Values;

        public void Add(object key, object? value) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public bool Contains(object key) => _info.Keys.Contains(key);

        public void CopyTo(Array array, int index) => throw new NotSupportedException();

        public IDictionaryEnumerator GetEnumerator() => new PocoDictionaryEnumerator(this);

        public void Remove(object key) => throw new NotSupportedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class PocoDictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly PocoDictionary _dict;
            private int _index = -1;

            public PocoDictionaryEnumerator(PocoDictionary dict)
            {
                _dict = dict;
            }

            public object Current => Entry;

            public DictionaryEntry Entry => new DictionaryEntry(_dict._info.Keys[_index], _dict.Values[_index]);

            public object Key => _dict._info.Keys[_index];

            public object? Value => _dict.Values[_index];

            public bool MoveNext()
            {
                _index++;
                return _index < _dict._info.Keys.Length;
            }

            public void Reset() => _index = -1;
        }
    }
}
