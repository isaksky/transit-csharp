```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                     | Mean       | Error     | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |-----------:|----------:|---------:|------:|-------:|-------:|----------:|------------:|
| OldTransit_ReadJson        | 1,921.6 ns | 344.35 ns | 18.87 ns |  1.00 | 2.9297 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJson        |   610.0 ns |  11.70 ns |  0.64 ns |  0.32 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
| OldTransit_ReadJsonVerbose | 1,902.7 ns |  34.00 ns |  1.86 ns |  0.99 | 2.9297 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJsonVerbose |   610.9 ns |  76.02 ns |  4.17 ns |  0.32 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
