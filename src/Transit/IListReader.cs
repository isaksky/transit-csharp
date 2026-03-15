namespace Transit;

/// <summary>
/// Builds lists during transit reading.
/// </summary>
public interface IListReader
{
    /// <summary>Initializes a new list.</summary>
    object Init();

    /// <summary>Adds an item to the list.</summary>
    object Add(object list, object item);

    /// <summary>Completes and returns the list.</summary>
    object Complete(object list);
}
