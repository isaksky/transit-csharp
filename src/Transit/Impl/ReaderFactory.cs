using System.Collections.Frozen;
using System.Text.Json;
using Transit.Impl.ReadHandlers;
using Transit.Spi;

namespace Transit.Impl;

/// <summary>
/// Constructs transit readers using System.Text.Json.
/// </summary>
internal static class ReaderFactory
{
    private static readonly FrozenDictionary<string, IReadHandler> DefaultHandlersInstance = BuildDefaultHandlers();

    private static FrozenDictionary<string, IReadHandler> BuildDefaultHandlers()
    {
        var dict = new Dictionary<string, IReadHandler>
        {
            [":" ] = new KeywordReadHandler(),
            ["$" ] = new SymbolReadHandler(),
            ["i" ] = new IntegerReadHandler(),
            ["?" ] = new BooleanReadHandler(),
            ["_" ] = new NullReadHandler(),
            ["f" ] = new BigRationalReadHandler(),
            ["n" ] = new BigIntegerReadHandler(),
            ["d" ] = new DoubleReadHandler(),
            ["z" ] = new SpecialNumberReadHandler(),
            ["c" ] = new CharacterReadHandler(),
            ["t" ] = new VerboseDateTimeReadHandler(),
            ["m" ] = new DateTimeReadHandler(),
            ["r" ] = new UriReadHandler(),
            ["u" ] = new GuidReadHandler(),
            ["b" ] = new BinaryReadHandler(),
            ["'" ] = new IdentityReadHandler(),
            ["set" ] = new SetReadHandler(),
            ["list"] = new ListReadHandler(),
            ["ratio"] = new RatioReadHandler(),
            ["cmap"] = new CDictionaryReadHandler(),
            ["link"] = new LinkReadHandler(),
        };
        return dict.ToFrozenDictionary();
    }

    public static FrozenDictionary<string, IReadHandler> DefaultHandlers() => DefaultHandlersInstance;

    public static IDefaultReadHandler<ITaggedValue> DefaultDefaultHandler() => new DefaultReadHandler();

    private static void DisallowOverridingGroundTypes(IDictionary<string, IReadHandler>? handlers)
    {
        if (handlers == null) return;

        string[] groundTypeTags = ["_", "s", "?", "i", "d", "b", "'", "map", "array"];
        foreach (var tag in groundTypeTags)
        {
            if (handlers.ContainsKey(tag))
                throw new TransitException("Cannot override decoding for transit ground type, tag " + tag);
        }
    }

    private static FrozenDictionary<string, IReadHandler> Handlers(IDictionary<string, IReadHandler>? customHandlers)
    {
        DisallowOverridingGroundTypes(customHandlers);

        if (customHandlers == null || customHandlers.Count == 0)
            return DefaultHandlersInstance;

        var dict = DefaultHandlersInstance.ToDictionary();
        foreach (var kvp in customHandlers)
            dict[kvp.Key] = kvp.Value;
        return dict.ToFrozenDictionary();
    }

    private static IDefaultReadHandler<object> DefaultHandler(IDefaultReadHandler<object>? customDefaultHandler)
        => customDefaultHandler ?? DefaultDefaultHandler();

    public static IReader GetJsonInstance(
        Stream input,
        IDictionary<string, IReadHandler>? customHandlers,
        IDefaultReadHandler<object>? customDefaultHandler)
    {
        return new JsonReader(input, Handlers(customHandlers), DefaultHandler(customDefaultHandler));
    }

    public static IReader GetMsgPackInstance(
        Stream input,
        IDictionary<string, IReadHandler>? customHandlers,
        IDefaultReadHandler<object>? customDefaultHandler)
    {
        throw new NotImplementedException("MessagePack is not yet implemented.");
    }

    private class Reader : IReader, IReaderSpi
    {
        protected Stream Input;
        protected FrozenDictionary<string, IReadHandler> HandlersDict;
        protected IDefaultReadHandler<object> DefaultHandlerInst;
        protected IDictionaryReader? DictBuilder;
        protected IListReader? LstBuilder;
        private ReadCache _cache;
        private IParser? _parser;
        private bool _initialized;

        public Reader(Stream input, FrozenDictionary<string, IReadHandler> handlers, IDefaultReadHandler<object> defaultHandler)
        {
            _initialized = false;
            Input = input;
            HandlersDict = handlers;
            DefaultHandlerInst = defaultHandler;
            _cache = new ReadCache();
        }

        public T Read<T>()
        {
            if (!_initialized)
                Initialize();

            return (T)_parser!.Parse(_cache.Init())!;
        }

        public IReader SetBuilders(IDictionaryReader dictionaryBuilder, IListReader listBuilder)
        {
            if (_initialized)
                throw new TransitException("Cannot set builders after read has been called.");

            DictBuilder = dictionaryBuilder;
            LstBuilder = listBuilder;
            return this;
        }

        private void EnsureBuilders()
        {
            DictBuilder ??= new DictionaryBuilder();
            LstBuilder ??= new ListBuilder();
        }

        protected void Initialize()
        {
            EnsureBuilders();
            _parser = CreateParser();
            _initialized = true;
        }

        protected virtual IParser CreateParser()
            => throw new NotImplementedException();
    }

    private sealed class JsonReader : Reader
    {
        public JsonReader(Stream input, FrozenDictionary<string, IReadHandler> handlers, IDefaultReadHandler<object> defaultHandler)
            : base(input, handlers, defaultHandler)
        {
        }

        protected override IParser CreateParser()
        {
            // Parse the entire stream into a JsonDocument
            var doc = JsonDocument.Parse(Input, new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            });

            return new JsonParser(doc.RootElement, HandlersDict, DefaultHandlerInst,
                DictBuilder!, LstBuilder!);
        }
    }
}
