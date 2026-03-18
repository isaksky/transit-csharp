namespace Transit.Net;

/// <summary>
/// Provides transit encoding for a specific type.
/// </summary>
public interface IWriteHandler
{
    /// <summary>Returns the tag for the given object.</summary>
    string Tag(object obj);

    /// <summary>Returns the representation of the object.</summary>
    object Representation(object obj);

    /// <summary>Returns the string representation of the object, or null.</summary>
    string? StringRepresentation(object obj);

    /// <summary>Returns a verbose-mode handler, or null to use this handler.</summary>
    IWriteHandler? GetVerboseHandler();
}
