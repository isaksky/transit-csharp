```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                     | Mean       | Error     | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |-----------:|----------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| OldTransit_ReadJson        | 1,839.5 ns |  66.03 ns |  3.62 ns |  1.00 |    0.00 | 2.9316 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJson        |   599.9 ns |  28.24 ns |  1.55 ns |  0.33 |    0.00 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
| OldTransit_ReadJsonVerbose | 1,834.2 ns | 550.54 ns | 30.18 ns |  1.00 |    0.02 | 2.9316 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJsonVerbose |   596.5 ns |  13.89 ns |  0.76 ns |  0.32 |    0.00 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
