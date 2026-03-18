namespace Transit.Net;

/// <summary>
/// Represents a tagged value for transit extension types.
/// </summary>
public interface ITaggedValue
{
    /// <summary>Gets the tag.</summary>
    string Tag { get; }

    /// <summary>Gets the representation.</summary>
    object Representation { get; }
}
