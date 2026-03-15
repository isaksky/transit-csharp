namespace Transit.Impl;

/// <summary>
/// Utility methods for transit encoding.
/// </summary>
internal static class Util
{
    /// <summary>
    /// Converts a boxed numeric type to a long.
    /// </summary>
    public static long NumberToPrimitiveLong(object o) => o switch
    {
        long l => l,
        int i => i,
        short s => s,
        byte b => b,
        _ => throw new TransitException("Unknown integer type: " + o.GetType())
    };

    /// <summary>
    /// Concatenates optional prefix + tag + value into a single string
    /// using string.Create to avoid StringBuilder allocations.
    /// </summary>
    public static string MaybePrefix(string? prefix, string? tag, string s)
    {
        if (prefix is null && tag is null)
            return s;

        prefix ??= "";
        tag ??= "";
        int len = prefix.Length + tag.Length + s.Length;

        return string.Create(len, (prefix, tag, s), static (span, state) =>
        {
            state.prefix.AsSpan().CopyTo(span);
            state.tag.AsSpan().CopyTo(span[state.prefix.Length..]);
            state.s.AsSpan().CopyTo(span[(state.prefix.Length + state.tag.Length)..]);
        });
    }
}
