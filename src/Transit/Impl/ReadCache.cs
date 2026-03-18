namespace Transit.Net.Impl;

/// <summary>
/// Cache for transit read decoding. Uses a fixed-size object array.
/// </summary>
internal sealed class ReadCache
{
    private readonly object[] _cache;
    private int _index;

    public ReadCache()
    {
        _cache = new object[WriteCache.MaxCacheEntries];
        _index = 0;
    }

    private static bool IsCacheCode(string s)
        => s[0] == Constants.Sub && !s.Equals(Constants.DirectoryAsList, StringComparison.Ordinal);

    private static int CodeToIndex(string s)
    {
        if (s.Length == 2)
            return s[1] - WriteCache.BaseCharIdx;

        return ((s[1] - WriteCache.BaseCharIdx) * WriteCache.CacheCodeDigits) +
               (s[2] - WriteCache.BaseCharIdx);
    }

    public object CacheRead(string s, bool asDictionaryKey)
        => CacheRead(s, asDictionaryKey, null);

    public object CacheRead(string s, bool asDictionaryKey, AbstractParser? p)
    {
        if (s.Length != 0)
        {
            if (IsCacheCode(s))
                return _cache[CodeToIndex(s)];

            if (WriteCache.IsCacheable(s, asDictionaryKey))
            {
                if (_index == WriteCache.MaxCacheEntries)
                    Init();

                return _cache[_index++] = (p != null ? p.ParseString(s) : s);
            }
        }

        return p != null ? p.ParseString(s) : s;
    }

    public ReadCache Init()
    {
        if (_index > 0)
            Array.Clear(_cache, 0, _index);
        _index = 0;
        return this;
    }
}
