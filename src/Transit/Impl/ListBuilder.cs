namespace Transit.Impl;

/// <summary>
/// Default list builder for transit reading.
/// </summary>
internal sealed class ListBuilder : IListReader
{
    public object Init() => new List<object>();

    public object Add(object list, object item)
    {
        ((List<object>)list).Add(item);
        return list;
    }

    public object Complete(object list) => list;
}
