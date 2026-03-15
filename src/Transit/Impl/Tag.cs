namespace Transit.Impl;

/// <summary>
/// Represents a tag used during parsing.
/// </summary>
internal sealed class Tag
{
    private readonly string _value;

    public Tag(string value) => _value = value;
    public string GetValue() => _value;
    public override string ToString() => _value;
}
