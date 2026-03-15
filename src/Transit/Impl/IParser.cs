namespace Transit.Impl;

/// <summary>
/// Internal parser interface.
/// </summary>
internal interface IParser
{
    object? Parse(ReadCache cache);
}
