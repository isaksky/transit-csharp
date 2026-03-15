using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace Transit.Benchmarks;

/// <summary>
/// Truly quick benchmark config: runs in-process (no child process spawn),
/// 1 warmup iteration, 3 actual iterations, 1 launch. Total ~seconds not minutes.
/// </summary>
public class QuickConfig : ManualConfig
{
    public QuickConfig()
    {
        AddJob(Job.Dry
            .WithToolchain(InProcessNoEmitToolchain.Instance)
            .WithWarmupCount(1)
            .WithIterationCount(3)
            .WithLaunchCount(1));
    }
}
