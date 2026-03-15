using System.Collections.Frozen;
using System.Globalization;

namespace Transit.Impl;

/// <summary>
/// Base class for transit parsers. Uses FrozenDictionary for handler lookup.
/// </summary>
internal abstract class AbstractParser : IParser
{
    public static string FormatDateTime(DateTime value)
    {
        const string dateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        return new DateTimeOffset(value).UtcDateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
    }

    protected readonly FrozenDictionary<string, IReadHandler> Handlers;
    private readonly IDefaultReadHandler<object>? _defaultHandler;
    protected IDictionaryReader DictionaryBuilder;
    protected IListReader ListBuilder;

    protected AbstractParser(
        FrozenDictionary<string, IReadHandler> handlers,
        IDefaultReadHandler<object>? defaultHandler,
        IDictionaryReader dictionaryBuilder,
        IListReader listBuilder)
    {
        Handlers = handlers;
        _defaultHandler = defaultHandler;
        DictionaryBuilder = dictionaryBuilder;
        ListBuilder = listBuilder;
    }

    protected bool TryGetHandler(string tag, out IReadHandler? readHandler)
        => Handlers.TryGetValue(tag, out readHandler);

    protected object Decode(string tag, object representation)
    {
        if (Handlers.TryGetValue(tag, out var handler))
            return handler.FromRepresentation(representation);

        if (_defaultHandler != null)
            return _defaultHandler.FromRepresentation(tag, representation);

        throw new TransitException($"Cannot FromRepresentation {tag}: {representation}");
    }

    // Pre-allocated single-char strings for tag lookup (printable ASCII '!' to '~')
    private static readonly string[] SingleCharStrings = InitSingleCharStrings();
    private static string[] InitSingleCharStrings()
    {
        var arr = new string[127];
        for (int i = '!'; i <= '~'; i++)
            arr[i] = ((char)i).ToString();
        return arr;
    }

    private static string SingleCharString(char c)
        => c < 127 ? SingleCharStrings[c] : c.ToString();

    internal object ParseString(object obj)
    {
        if (obj is string s && s.Length > 1)
        {
            switch (s[0])
            {
                case Constants.Esc:
                    switch (s[1])
                    {
                        case Constants.Esc:
                        case Constants.Sub:
                        case Constants.Reserved:
                            return s[1..];
                        case Constants.Tag:
                            return new Tag(s[2..]);
                        default:
                            var tag = SingleCharString(s[1]);
                            var representation = s.Length > 2 ? s[2..] : string.Empty;
                            return Decode(tag, representation);
                    }
                case Constants.Sub:
                    if (s[1] == ' ')
                        return Constants.DirectoryAsList;
                    break;
            }
        }

        return obj;
    }

    public abstract object? Parse(ReadCache cache);
    public abstract object? ParseVal(bool asDictionaryKey, ReadCache cache);
    public abstract object? ParseDictionary(bool asDictionaryKey, ReadCache cache, IDictionaryReadHandler? handler);
    public abstract object? ParseList(bool asDictionaryKey, ReadCache cache, IListReadHandler? handler);
}
