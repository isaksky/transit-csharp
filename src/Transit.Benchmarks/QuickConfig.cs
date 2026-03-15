using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Transit.Benchmarks;

/// <summary>
/// Quick but reliable benchmark config: runs in-process (no child process spawn),
/// uses ShortRun with Throughput strategy so BenchmarkDotNet auto-tunes the
/// invocation count to get stable measurements. Typically completes in ~1-2 minutes.
/// </summary>
public class QuickConfig : ManualConfig
{
    public QuickConfig()
    {
        AddJob(Job.ShortRun
            .WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}
