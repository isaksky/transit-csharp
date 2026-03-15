namespace Transit;

/// <summary>
/// A read handler that can also decode dictionary representations.
/// </summary>
public interface IDictionaryReadHandler : IReadHandler
{
    /// <summary>Returns a dictionary reader for decoding the map representation.</summary>
    IDictionaryReader DictionaryReader();
}
