namespace Transit.Net.Impl;

/// <summary>
/// Represents a transit keyword as an allocation-light value type.
/// </summary>
internal readonly struct Keyword : IKeyword, INamed, IComparable<IKeyword>, IEquatable<IKeyword>
{
    private const char Separator = '/';
    private readonly string _value;
    private readonly int _separatorIndex; // -1 if no namespace

    public Keyword(string nsname)
    {
        _value = nsname;
        _separatorIndex = nsname.IndexOf(Separator);
        if (_separatorIndex >= 0 && nsname.Length == 1)
            _separatorIndex = -1; // "/" alone means no namespace
    }

    public string Value => _value;
    public string Name => _separatorIndex < 0 ? _value : _value[(_separatorIndex + 1)..];
    public string? Namespace => _separatorIndex < 0 ? null : _value[.._separatorIndex];

    public override string ToString() => _value;
    public override int GetHashCode() => _value.GetHashCode();
    public override bool Equals(object? obj) => obj is IKeyword other && _value == other.Value;
    public bool Equals(IKeyword? other) => other is not null && _value == other.Value;
    public int CompareTo(IKeyword? other) => other is null ? 1 : string.Compare(_value, other.Value, StringComparison.Ordinal);

    public static bool operator ==(Keyword left, Keyword right) => left._value == right._value;
    public static bool operator !=(Keyword left, Keyword right) => left._value != right._value;
}
