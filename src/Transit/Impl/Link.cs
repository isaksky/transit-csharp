namespace Transit.Impl;

/// <summary>
/// Represents a transit link.
/// </summary>
internal sealed class Link : ILink
{
    public Uri Href { get; }
    public string Rel { get; }
    public string? Name { get; }
    public string? Prompt { get; }
    public string? Render { get; }

    public Link(Uri href, string rel, string? name = null, string? prompt = null, string? render = null)
    {
        Href = href;
        Rel = rel;
        Name = name;
        Prompt = prompt;
        Render = render;
    }

    public override string ToString() => $"Link({Href}, {Rel})";
}
