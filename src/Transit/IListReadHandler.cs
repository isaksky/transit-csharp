namespace Transit;

/// <summary>
/// A read handler that can also decode list representations.
/// </summary>
public interface IListReadHandler : IReadHandler
{
    /// <summary>Returns a list reader for decoding the list representation.</summary>
    IListReader ListReader();
}
