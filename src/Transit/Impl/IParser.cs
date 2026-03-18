namespace Transit.Net.Impl;

/// <summary>
/// Internal parser interface.
/// </summary>
internal interface IParser
{
    object? Parse(ReadCache cache);
}
