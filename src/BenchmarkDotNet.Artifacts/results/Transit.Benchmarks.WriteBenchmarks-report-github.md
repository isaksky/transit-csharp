```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                      | Mean       | Error     | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|---------:|------:|-------:|-------:|----------:|------------:|
| OldTransit_WriteJson        | 2,666.6 ns |  10.37 ns |  0.57 ns |  1.00 | 0.9995 | 0.0191 |   8.18 KB |        1.00 |
| NewTransit_WriteJson        |   833.0 ns |  14.67 ns |  0.80 ns |  0.31 | 0.4177 | 0.0048 |   3.42 KB |        0.42 |
| OldTransit_WriteJsonVerbose | 4,271.6 ns | 307.88 ns | 16.88 ns |  1.60 | 1.1749 | 0.0229 |   9.64 KB |        1.18 |
| NewTransit_WriteJsonVerbose | 1,689.0 ns |  15.52 ns |  0.85 ns |  0.63 | 0.7935 | 0.0153 |   6.49 KB |        0.79 |
