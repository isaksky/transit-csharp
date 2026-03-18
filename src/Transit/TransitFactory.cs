using System.Collections.Frozen;
using Transit.Net.Impl;
using Transit.Net.Spi;
using System.Numerics;

namespace Transit.Net;

/// <summary>
/// Main entry point for the Transit library. Provides methods to construct
/// readers and writers, as well as helpers to create transit values.
/// </summary>
public static class TransitFactory
{
    /// <summary>
    /// Transit formats.
    /// </summary>
    public enum Format
    {
        Json,
        MsgPack,
        JsonVerbose
    }

    /// <summary>
    /// Pre-merges custom read handlers with the default Transit handlers and returns a FrozenDictionary.
    /// This allows caching the merged handlers map to avoid overhead on repeated reader creation.
    /// </summary>
    public static FrozenDictionary<string, IReadHandler> MergedReadHandlers(IDictionary<string, IReadHandler>? customHandlers)
        => Impl.ReaderFactory.MergedHandlers(customHandlers);

    /// <summary>
    /// Pre-merges custom write handlers with the default Transit handlers and returns a FrozenDictionary.
    /// This allows caching the merged handlers map to avoid overhead on repeated writer creation.
    /// </summary>
    public static FrozenDictionary<Type, IWriteHandler> MergedWriteHandlers(IDictionary<Type, IWriteHandler>? customHandlers)
        => Impl.WriterFactory.MergedHandlers(customHandlers);

    /// <summary>
    /// Creates a writer instance.
    /// </summary>
    public static IWriter<T> Writer<T>(Format type, Stream output, bool ownsStream = true)
        => Writer<T>(type, output, null, ownsStream);

    /// <summary>
    /// Creates a writer instance with custom handlers.
    /// </summary>
    public static IWriter<T> Writer<T>(Format type, Stream output, IDictionary<Type, IWriteHandler>? customHandlers, bool ownsStream = true)
        => Writer<T>(type, output, customHandlers, null, null, ownsStream);

    /// <summary>
    /// Creates a writer instance with custom handlers and a default handler.
    /// </summary>
    public static IWriter<T> Writer<T>(Format type, Stream output, IDictionary<Type, IWriteHandler>? customHandlers, IWriteHandler? defaultWriteHandler, bool ownsStream = true)
        => Writer<T>(type, output, customHandlers, defaultWriteHandler, null, ownsStream);

    /// <summary>
    /// Creates a writer instance with custom handlers, a default handler, and a write-time transform function.
    /// </summary>
    public static IWriter<T> Writer<T>(Format type, Stream output, IDictionary<Type, IWriteHandler>? customHandlers, IWriteHandler? defaultWriteHandler, Func<object, object>? transform, bool ownsStream = true)
    {
        return type switch
        {
            Format.MsgPack => WriterFactory.GetMsgPackInstance<T>(output, customHandlers, ownsStream, defaultWriteHandler, transform),
            Format.Json => WriterFactory.GetJsonInstance<T>(output, customHandlers, false, ownsStream, defaultWriteHandler, transform),
            Format.JsonVerbose => WriterFactory.GetJsonInstance<T>(output, customHandlers, true, ownsStream, defaultWriteHandler, transform),
            _ => throw new ArgumentException("Unknown Writer type: " + type)
        };
    }

    /// <summary>
    /// Creates a reader instance.
    /// </summary>
    public static IReader Reader(Format type, Stream input, bool ownsStream = true)
        => Reader(type, input, null, DefaultDefaultReadHandler(), ownsStream);

    /// <summary>
    /// Creates a reader instance with a custom default handler.
    /// </summary>
    public static IReader Reader(Format type, Stream input, IDefaultReadHandler<object> customDefaultHandler, bool ownsStream = true)
        => Reader(type, input, null, customDefaultHandler, ownsStream);

    /// <summary>
    /// Creates a reader instance with custom handlers.
    /// </summary>
    public static IReader Reader(Format type, Stream input, IDictionary<string, IReadHandler> customHandlers, bool ownsStream = true)
        => Reader(type, input, customHandlers, DefaultDefaultReadHandler(), ownsStream);

    /// <summary>
    /// Creates a reader instance with custom handlers and a custom default handler.
    /// </summary>
    public static IReader Reader(Format type, Stream input,
        IDictionary<string, IReadHandler>? customHandlers,
        IDefaultReadHandler<object>? customDefaultHandler,
        bool ownsStream = true)
    {
        return type switch
        {
            Format.Json or Format.JsonVerbose =>
                ReaderFactory.GetJsonInstance(input, customHandlers, customDefaultHandler, ownsStream),
            Format.MsgPack =>
                ReaderFactory.GetMsgPackInstance(input, customHandlers, customDefaultHandler, ownsStream),
            _ => throw new ArgumentException("Unknown Reader type: " + type)
        };
    }

    /// <summary>
    /// Converts a string or IKeyword to an IKeyword.
    /// </summary>
    public static IKeyword Keyword(object obj)
    {
        if (obj is IKeyword kw) return kw;
        if (obj is string s)
        {
            if (s.Length > 0 && s[0] == ':')
                return new Keyword(s[1..]);
            return new Keyword(s);
        }
        throw new TransitException("Cannot make keyword from " + obj.GetType());
    }

    /// <summary>
    /// Converts a string or ISymbol to an ISymbol.
    /// </summary>
    public static ISymbol Symbol(object obj)
    {
        if (obj is ISymbol sym) return sym;
        if (obj is string s)
        {
            if (s.Length > 0 && s[0] == ':')
                return new Symbol(s[1..]);
            return new Symbol(s);
        }
        throw new TransitException("Cannot make symbol from " + obj.GetType());
    }

    /// <summary>
    /// Creates a tagged value.
    /// </summary>
    public static ITaggedValue TaggedValue(string tag, object representation)
        => new TaggedValue(tag, representation);

    /// <summary>
    /// Creates a link.
    /// </summary>
    public static ILink Link(string href, string rel)
        => Link(new Uri(href), rel, null, null, null);

    /// <summary>
    /// Creates a link.
    /// </summary>
    public static ILink Link(Uri href, string rel)
        => Link(href, rel, null, null, null);

    /// <summary>
    /// Creates a link with all parameters.
    /// </summary>
    public static ILink Link(string href, string rel, string? name, string? prompt, string? render)
        => Link(new Uri(href), rel, name, prompt, render);

    /// <summary>
    /// Creates a link with all parameters.
    /// </summary>
    public static ILink Link(Uri href, string rel, string? name, string? prompt, string? render)
        => new Impl.Link(href, rel, name, prompt, render);

    /// <summary>
    /// Creates a ratio.
    /// </summary>
    public static IRatio Ratio(BigInteger numerator, BigInteger denominator)
        => new Impl.Ratio(numerator, denominator);

    /// <summary>
    /// Returns the default read handlers.
    /// </summary>
    public static IDictionary<string, IReadHandler> DefaultReadHandlers()
        => ReaderFactory.DefaultHandlers().ToDictionary();

    /// <summary>
    /// Returns the default write handlers.
    /// </summary>
    public static IDictionary<Type, IWriteHandler> DefaultWriteHandlers()
        => WriterFactory.DefaultHandlers().ToDictionary();

    /// <summary>
    /// Returns the default default read handler.
    /// </summary>
    public static IDefaultReadHandler<ITaggedValue> DefaultDefaultReadHandler()
        => ReaderFactory.DefaultDefaultHandler();
}
