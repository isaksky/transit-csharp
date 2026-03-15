namespace Transit;

/// <summary>
/// Reads transit-encoded data.
/// </summary>
public interface IReader
{
    /// <summary>
    /// Reads and returns a value.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <returns>A value.</returns>
    T Read<T>();
}
