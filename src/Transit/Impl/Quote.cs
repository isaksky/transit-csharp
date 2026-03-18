namespace Transit.Net.Impl;

/// <summary>
/// Wraps a scalar value for top-level encoding.
/// </summary>
internal sealed class Quote
{
    public object? Value { get; }
    public Quote(object? value) => Value = value;
}
