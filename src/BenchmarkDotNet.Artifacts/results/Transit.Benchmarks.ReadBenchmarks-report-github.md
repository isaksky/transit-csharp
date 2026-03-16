```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                     | Mean       | Error     | StdDev  | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|--------------------------- |-----------:|----------:|--------:|------:|-------:|-------:|----------:|------------:|
| OldTransit_ReadJson        | 1,930.2 ns | 178.13 ns | 9.76 ns |  1.00 | 2.9297 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJson        |   598.2 ns |  25.40 ns | 1.39 ns |  0.31 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
| OldTransit_ReadJsonVerbose | 1,923.9 ns |  74.16 ns | 4.06 ns |  1.00 | 2.9297 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJsonVerbose |   598.5 ns |  26.72 ns | 1.46 ns |  0.31 | 1.9798 | 0.0610 |  16.21 KB |        0.68 |
