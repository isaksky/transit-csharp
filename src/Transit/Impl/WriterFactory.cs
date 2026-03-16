using System.Buffers;
using System.Collections;
using System.Collections.Frozen;
using System.Numerics;
using System.Text.Json;
using Transit.Impl.WriteHandlers;
using Transit.Numerics;
using Transit.Spi;

namespace Transit.Impl;

/// <summary>
/// Constructs transit writers. Handler dictionaries are built once as FrozenDictionary.
/// </summary>
internal static class WriterFactory
{
    private static readonly FrozenDictionary<Type, IWriteHandler> DefaultHandlersInstance = BuildDefaultHandlers();

    private static FrozenDictionary<Type, IWriteHandler> BuildDefaultHandlers()
    {
        var integerHandler = new IntegerWriteHandler("i");
        var dict = new Dictionary<Type, IWriteHandler>
        {
            [typeof(bool)] = new BooleanWriteHandler(),
            [typeof(NullType)] = new NullWriteHandler(),
            [typeof(string)] = new ToStringWriteHandler("s"),
            [typeof(int)] = integerHandler,
            [typeof(long)] = integerHandler,
            [typeof(short)] = integerHandler,
            [typeof(byte)] = integerHandler,
            [typeof(BigInteger)] = new ToStringWriteHandler("n"),
            [typeof(decimal)] = new ToStringWriteHandler("f"),
            [typeof(BigRational)] = new ToStringWriteHandler("f"),
            [typeof(float)] = new FloatWriteHandler(),
            [typeof(double)] = new DoubleWriteHandler(),
            [typeof(char)] = new ToStringWriteHandler("c"),
            [typeof(IKeyword)] = new ToStringWriteHandler(":"),
            [typeof(ISymbol)] = new ToStringWriteHandler("$"),
            [typeof(byte[])] = new BinaryWriteHandler(),
            [typeof(Guid)] = new GuidWriteHandler(),
            [typeof(Uri)] = new ToStringWriteHandler("r"),
            [typeof(DateTime)] = new DateTimeWriteHandler(),
            [typeof(IRatio)] = new RatioWriteHandler(),
            [typeof(ILink)] = new LinkWriteHandler(),
            [typeof(Quote)] = new QuoteWriteHandler(),
            [typeof(ITaggedValue)] = new TaggedValueWriteHandler(),
            [typeof(ISet<>)] = new SetWriteHandler(),
            [typeof(IEnumerable)] = new EnumerableWriteHandler(),
            [typeof(IList<>)] = new ListWriteHandler(),
            [typeof(IDictionary<,>)] = new DictionaryWriteHandler(),
            [typeof(NullKeyDictionary)] = new DictionaryWriteHandler(),
        };
        return dict.ToFrozenDictionary();
    }

    public static FrozenDictionary<Type, IWriteHandler> DefaultHandlers() => DefaultHandlersInstance;

    private static FrozenDictionary<Type, IWriteHandler> Handlers(IDictionary<Type, IWriteHandler>? customHandlers)
    {
        if (customHandlers == null || customHandlers.Count == 0)
            return DefaultHandlersInstance;

        var dict = DefaultHandlersInstance.ToDictionary();
        foreach (var kvp in customHandlers)
            dict[kvp.Key] = kvp.Value;
        return dict.ToFrozenDictionary();
    }

    private static void SetSubHandler(FrozenDictionary<Type, IWriteHandler> handlers, AbstractEmitter emitter)
    {
        foreach (var handler in handlers.Values)
        {
            if (handler is IAbstractEmitterAware aware)
                aware.SetEmitter(emitter);
        }
    }

    private static FrozenDictionary<Type, IWriteHandler> GetVerboseHandlers(FrozenDictionary<Type, IWriteHandler> handlers)
    {
        var dict = new Dictionary<Type, IWriteHandler>(handlers.Count);
        foreach (var item in handlers)
            dict[item.Key] = item.Value.GetVerboseHandler() ?? item.Value;
        return dict.ToFrozenDictionary();
    }

    public static IWriter<T> GetJsonInstance<T>(Stream output, IDictionary<Type, IWriteHandler>? customHandlers, bool verboseMode, bool ownsStream = true)
    {
        var handlers = Handlers(customHandlers);
        var bufferWriter = new ArrayBufferWriter<byte>();
        var jsonWriter = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions
        {
            SkipValidation = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        JsonEmitter emitter;
        if (verboseMode)
        {
            var verboseHandlers = GetVerboseHandlers(handlers);
            emitter = new JsonVerboseEmitter(jsonWriter, verboseHandlers);
            SetSubHandler(verboseHandlers, emitter);
        }
        else
        {
            emitter = new JsonEmitter(jsonWriter, handlers);
            SetSubHandler(handlers, emitter);
        }

        var wc = new WriteCache(!verboseMode);
        return new Writer<T>(output, emitter, wc, bufferWriter, ownsStream);
    }

    public static IWriter<T> GetMsgPackInstance<T>(Stream output, IDictionary<Type, IWriteHandler>? customHandlers, bool ownsStream = true)
        => throw new NotImplementedException("MessagePack is not yet implemented.");

    private sealed class Writer<T> : IWriter<T>
    {
        private readonly Stream _output;
        private readonly JsonEmitter _emitter;
        private readonly WriteCache _wc;
        private readonly ArrayBufferWriter<byte> _buf;
        private readonly bool _ownsStream;
        private bool _disposed;

        public Writer(Stream output, JsonEmitter emitter, WriteCache wc, ArrayBufferWriter<byte> buf, bool ownsStream)
        {
            _output = output;
            _emitter = emitter;
            _wc = wc;
            _buf = buf;
            _ownsStream = ownsStream;
        }

        public void Write(T value)
        {
            _emitter.Emit(value!, false, _wc.Init());
            _emitter.JsonWriter.Flush();
            _output.Write(_buf.WrittenSpan);
            _buf.ResetWrittenCount();
            _output.Flush();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _emitter.JsonWriter.Dispose();
            if (_ownsStream)
                _output.Dispose();
        }
    }
}
