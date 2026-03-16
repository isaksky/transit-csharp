extern alias OldTransit;
using System.IO;
using BenchmarkDotNet.Attributes;
using OldTransitFactory = OldTransit::Beerendonk.Transit.TransitFactory;
using NewTransitFactory = Transit.TransitFactory;

namespace Transit.Benchmarks;

[Config(typeof(QuickConfig))]
[MemoryDiagnoser]
public class CacheBenchmarks
{
    private byte[] _oldJsonWithCaching = Array.Empty<byte>();
    private byte[] _newJsonWithCaching = Array.Empty<byte>();

    [GlobalSetup]
    public void Setup()
    {
        var list = new List<object>();
        for (int i = 0; i < 1000; i++)
        {
            list.Add(new Dictionary<object, object> {
                { "~:cached-key-1", i },
                { "~:cached-key-2", "cached-string-value" },
                { "~scached-string-key", i * 2 }
            });
        }

        using (var ms = new MemoryStream())
        {
            var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.Json, ms);
            writer.Write(list);
            _oldJsonWithCaching = ms.ToArray();
        }

        using (var ms = new MemoryStream())
        {
            using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.Json, ms, ownsStream: false);
            writer.Write(list);
            _newJsonWithCaching = ms.ToArray();
        }
    }

    [Benchmark(Baseline = true)]
    public byte[] OldTransit_WriteCache()
    {
        var list = new List<object>();
        for (int i = 0; i < 1000; i++)
        {
            list.Add(new Dictionary<object, object> {
                { "~:cached-key-1", i },
                { "~:cached-key-2", "cached-string-value" },
                { "~scached-string-key", i * 2 }
            });
        }
        using var ms = new MemoryStream();
        var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.Json, ms);
        writer.Write(list);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] NewTransit_WriteCache()
    {
        var list = new List<object>();
        for (int i = 0; i < 1000; i++)
        {
            list.Add(new Dictionary<object, object> {
                { "~:cached-key-1", i },
                { "~:cached-key-2", "cached-string-value" },
                { "~scached-string-key", i * 2 }
            });
        }
        using var ms = new MemoryStream();
        using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.Json, ms, ownsStream: false);
        writer.Write(list);
        return ms.ToArray();
    }

    [Benchmark]
    public object OldTransit_ReadCache()
    {
        using var ms = new MemoryStream(_oldJsonWithCaching);
        var reader = OldTransitFactory.Reader(OldTransitFactory.Format.Json, ms);
        return reader.Read<object>();
    }

    [Benchmark]
    public object NewTransit_ReadCache()
    {
        using var ms = new MemoryStream(_newJsonWithCaching);
        using var reader = NewTransitFactory.Reader(NewTransitFactory.Format.Json, ms);
        return reader.Read<object>();
    }
}
