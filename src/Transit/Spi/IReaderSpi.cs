namespace Transit.Net.Spi;

/// <summary>
/// SPI interface for reader configuration.
/// </summary>
internal interface IReaderSpi
{
    /// <summary>
    /// Sets custom builders for dictionaries and lists.
    /// </summary>
    IReader SetBuilders(IDictionaryReader dictionaryBuilder, IListReader listBuilder);
}
