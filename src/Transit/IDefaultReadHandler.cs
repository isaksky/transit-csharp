namespace Transit.Net;

/// <summary>
/// Handles reading of unknown transit-encoded values.
/// </summary>
/// <typeparam name="T">The return type.</typeparam>
public interface IDefaultReadHandler<out T>
{
    /// <summary>
    /// Converts an unknown transit-encoded value.
    /// </summary>
    /// <param name="tag">The tag.</param>
    /// <param name="representation">The representation.</param>
    /// <returns>A decoded value.</returns>
    T FromRepresentation(string tag, object representation);
}
