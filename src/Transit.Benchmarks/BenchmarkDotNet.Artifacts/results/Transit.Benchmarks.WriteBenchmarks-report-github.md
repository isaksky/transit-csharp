```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                      | Mean     | Error     | StdDev    | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |---------:|----------:|----------:|------:|-------:|-------:|----------:|------------:|
| OldTransit_WriteJson        | 2.726 μs | 0.1624 μs | 0.0089 μs |  1.00 | 1.0071 | 0.0076 |   8.24 KB |        1.00 |
| NewTransit_WriteJson        | 2.161 μs | 0.1437 μs | 0.0079 μs |  0.79 | 8.1940 | 0.7820 |  67.13 KB |        8.14 |
| OldTransit_WriteJsonVerbose | 4.337 μs | 0.1549 μs | 0.0085 μs |  1.59 | 1.1826 | 0.0229 |    9.7 KB |        1.18 |
| NewTransit_WriteJsonVerbose | 1.741 μs | 0.0431 μs | 0.0024 μs |  0.64 | 0.7915 | 0.0172 |   6.48 KB |        0.79 |
