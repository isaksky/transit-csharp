namespace Transit.Impl;

/// <summary>
/// Cache for transit write encoding. Uses a mutable dictionary
/// (never shared across threads) for O(1) lookup.
/// </summary>
internal sealed class WriteCache
{
    public const int MinSizeCacheable = 4;
    public const int CacheCodeDigits = 44;
    public const int MaxCacheEntries = CacheCodeDigits * CacheCodeDigits;
    public const int BaseCharIdx = 48;

    private Dictionary<string, string>? _cache;
    private int _index;
    private readonly bool _enabled;

    public WriteCache() : this(true) { }

    public WriteCache(bool enabled)
    {
        _enabled = enabled;
        _index = 0;
        if (enabled)
            _cache = new Dictionary<string, string>(MaxCacheEntries);
    }

    public static bool IsCacheable(string s, bool asDictionaryKey)
    {
        return s.Length >= MinSizeCacheable &&
            (asDictionaryKey ||
                (s[0] == Constants.Esc &&
                (s[1] == ':' || s[1] == '$' || s[1] == '#')));
    }

    private static string IndexToCode(int index)
    {
        int hi = index / CacheCodeDigits;
        int lo = index % CacheCodeDigits;
        if (hi == 0)
            return string.Create(2, lo, static (span, lo) =>
            {
                span[0] = Constants.Sub;
                span[1] = (char)(lo + BaseCharIdx);
            });

        return string.Create(3, (hi, lo), static (span, state) =>
        {
            span[0] = Constants.Sub;
            span[1] = (char)(state.hi + BaseCharIdx);
            span[2] = (char)(state.lo + BaseCharIdx);
        });
    }

    public string CacheWrite(string s, bool asDictionaryKey)
    {
        if (_enabled && IsCacheable(s, asDictionaryKey))
        {
            if (_cache!.TryGetValue(s, out var val))
                return val;

            if (_index == MaxCacheEntries)
                Init();

            _cache[s] = IndexToCode(_index++);
        }

        return s;
    }

    public WriteCache Init()
    {
        _index = 0;
        _cache?.Clear();
        return this;
    }
}
