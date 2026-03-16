namespace Transit;

/// <summary>
/// Writes transit-encoded data.
/// </summary>
/// <typeparam name="T">The type of the value to write.</typeparam>
public interface IWriter<in T> : IDisposable
{
    /// <summary>
    /// Writes a value.
    /// </summary>
    /// <param name="value">The value to write.</param>
    void Write(T value);
}
