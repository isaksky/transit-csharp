namespace Transit.Impl;

/// <summary>
/// Default dictionary builder for transit reading.
/// </summary>
internal sealed class DictionaryBuilder : IDictionaryReader
{
    public object Init() => new Dictionary<object, object>();

    public object Add(object dictionary, object key, object value)
    {
        ((Dictionary<object, object>)dictionary)[key] = value;
        return dictionary;
    }

    public object Complete(object dictionary) => dictionary;
}
