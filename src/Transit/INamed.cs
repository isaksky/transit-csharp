namespace Transit;

/// <summary>
/// Provides namespace and name.
/// </summary>
public interface INamed
{
    /// <summary>Gets the name.</summary>
    string Name { get; }

    /// <summary>Gets the namespace, or null.</summary>
    string? Namespace { get; }
}
