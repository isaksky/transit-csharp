extern alias OldTransit;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using OldTransitFactory = OldTransit::Beerendonk.Transit.TransitFactory;
using NewTransitFactory = Transit.Net.TransitFactory;

namespace Transit.Net.Benchmarks;

[Config(typeof(QuickConfig))]
[MemoryDiagnoser]
public class ReadBenchmarks
{
    private byte[] _oldJsonPayload = Array.Empty<byte>();
    private byte[] _oldJsonVerbosePayload = Array.Empty<byte>();
    private byte[] _newJsonPayload = Array.Empty<byte>();
    private byte[] _newJsonVerbosePayload = Array.Empty<byte>();

    [GlobalSetup]
    public void Setup()
    {
        var data = new List<object> { 1, 2, 3, 4, 5, "hello", "Transit" };

        // Old payloads
        using (var ms = new MemoryStream())
        {
            var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.Json, ms);
            writer.Write(data);
            _oldJsonPayload = ms.ToArray();
        }
        using (var ms = new MemoryStream())
        {
            var writer = OldTransitFactory.Writer<object>(OldTransitFactory.Format.JsonVerbose, ms);
            writer.Write(data);
            _oldJsonVerbosePayload = ms.ToArray();
        }

        // New payloads
        using (var ms = new MemoryStream())
        {
            using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.Json, ms, ownsStream: false);
            writer.Write(data);
            _newJsonPayload = ms.ToArray();
        }
        using (var ms = new MemoryStream())
        {
            using var writer = NewTransitFactory.Writer<object>(NewTransitFactory.Format.JsonVerbose, ms, ownsStream: false);
            writer.Write(data);
            _newJsonVerbosePayload = ms.ToArray();
        }
    }

    [Benchmark(Baseline = true)]
    public object OldTransit_ReadJson()
    {
        using var ms = new MemoryStream(_oldJsonPayload);
        var reader = OldTransitFactory.Reader(OldTransitFactory.Format.Json, ms);
        return reader.Read<object>();
    }

    [Benchmark]
    public object NewTransit_ReadJson()
    {
        using var ms = new MemoryStream(_newJsonPayload);
        using var reader = NewTransitFactory.Reader(NewTransitFactory.Format.Json, ms);
        return reader.Read<object>();
    }

    [Benchmark]
    public object OldTransit_ReadJsonVerbose()
    {
        using var ms = new MemoryStream(_oldJsonVerbosePayload);
        var reader = OldTransitFactory.Reader(OldTransitFactory.Format.JsonVerbose, ms);
        return reader.Read<object>();
    }

    [Benchmark]
    public object NewTransit_ReadJsonVerbose()
    {
        using var ms = new MemoryStream(_newJsonVerbosePayload);
        using var reader = NewTransitFactory.Reader(NewTransitFactory.Format.JsonVerbose, ms);
        return reader.Read<object>();
    }
}
