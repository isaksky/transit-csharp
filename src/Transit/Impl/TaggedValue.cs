namespace Transit.Net.Impl;

/// <summary>
/// Represents a tagged value (extension type).
/// </summary>
internal sealed class TaggedValue : ITaggedValue, IEquatable<TaggedValue>
{
    public string Tag { get; }
    public object Representation { get; }

    public TaggedValue(string tag, object representation)
    {
        Tag = tag;
        Representation = representation;
    }

    public override bool Equals(object? obj) => obj is TaggedValue other && Equals(other);

    public bool Equals(TaggedValue? other) =>
        other is not null &&
        Tag == other.Tag &&
        Equals(Representation, other.Representation);

    public override int GetHashCode() => HashCode.Combine(Tag, Representation);
    public override string ToString() => $"TaggedValue({Tag}, {Representation})";
}
