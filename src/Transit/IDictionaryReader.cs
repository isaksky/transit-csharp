namespace Transit;

/// <summary>
/// Builds dictionaries during transit reading.
/// </summary>
public interface IDictionaryReader
{
    /// <summary>Initializes a new dictionary.</summary>
    object Init();

    /// <summary>Adds a key-value pair.</summary>
    object Add(object dictionary, object key, object value);

    /// <summary>Completes and returns the dictionary.</summary>
    object Complete(object dictionary);
}
