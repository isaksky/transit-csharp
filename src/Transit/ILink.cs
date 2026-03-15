namespace Transit;

/// <summary>
/// Represents a transit link.
/// </summary>
public interface ILink
{
    /// <summary>Gets the href.</summary>
    Uri Href { get; }

    /// <summary>Gets the rel.</summary>
    string Rel { get; }

    /// <summary>Gets the optional name.</summary>
    string? Name { get; }

    /// <summary>Gets the optional prompt.</summary>
    string? Prompt { get; }

    /// <summary>Gets the optional render.</summary>
    string? Render { get; }
}
