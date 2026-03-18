extern alias OldTransit;
using System.IO;
using BenchmarkDotNet.Attributes;
using OldTransitFactory = OldTransit::Beerendonk.Transit.TransitFactory;
using NewTransitFactory = Transit.Net.TransitFactory;

namespace Transit.Net.Benchmarks;

[Config(typeof(QuickConfig))]
[MemoryDiagnoser]
public class WriteBenchmarks
{
    private object _dataPayload = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dataPayload = new List<object>
        {
            1, 2, 3, 4, 5,
            "hello",
            "Transit",
            new Dictionary<object, object> {
                { "key1", "value1" },
                { "key2", 42 }
            }
        };
    }

    [Benchmark(Baseline = true)]
    public byte[] OldTransit_WriteJson()
    {
        using var ms = new MemoryStream();
        var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.Json, ms);
        writer.Write(_dataPayload);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] NewTransit_WriteJson()
    {
        using var ms = new MemoryStream();
        using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.Json, ms, ownsStream: false);
        writer.Write(_dataPayload);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] OldTransit_WriteJsonVerbose()
    {
        using var ms = new MemoryStream();
        var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.JsonVerbose, ms);
        writer.Write(_dataPayload);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] NewTransit_WriteJsonVerbose()
    {
        using var ms = new MemoryStream();
        using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.JsonVerbose, ms, ownsStream: false);
        writer.Write(_dataPayload);
        return ms.ToArray();
    }

    [Benchmark]
    public byte[] NewTransit_WriteArray()
    {
        var array = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        using var ms = new MemoryStream();
        using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.Json, ms, ownsStream: false);
        writer.Write(array);
        return ms.ToArray();
    }
}
