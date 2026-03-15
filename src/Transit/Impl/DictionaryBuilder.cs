namespace Transit.Impl;

/// <summary>
/// Default dictionary builder for transit reading.
/// </summary>
internal sealed class DictionaryBuilder : IDictionaryReader
{
    public object Init() => new NullKeyDictionary();

    public object Add(object dictionary, object key, object value)
    {
        ((System.Collections.IDictionary)dictionary)[key] = value;
        return dictionary;
    }

    public object Complete(object dictionary) => dictionary;
}
