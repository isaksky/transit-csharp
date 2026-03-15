namespace Transit.Impl;

/// <summary>
/// Transit protocol constants.
/// </summary>
internal static class Constants
{
    public const char Esc = '~';
    public const string EscStr = "~";
    public const char Tag = '#';
    public const string TagStr = "#";
    public const char Sub = '^';
    public const string SubStr = "^";
    public const char Reserved = '`';
    public const string EscTag = EscStr + TagStr;
    public const string QuoteTag = EscTag + "'";
    public const string DirectoryAsList = "^ ";
}
