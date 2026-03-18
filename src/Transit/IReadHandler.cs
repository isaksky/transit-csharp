namespace Transit.Net;

/// <summary>
/// Decodes a transit-encoded value from its representation.
/// </summary>
public interface IReadHandler
{
    /// <summary>
    /// Converts the transit representation to a typed value.
    /// </summary>
    /// <param name="representation">The transit representation.</param>
    /// <returns>The decoded value.</returns>
    object FromRepresentation(object representation);
}
