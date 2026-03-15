```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                  | Mean          | Error       | StdDev     | Median        | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|------------------------ |--------------:|------------:|-----------:|--------------:|------:|--------:|-------:|-------:|----------:|------------:|
| OldTransit_WriteJson    | 2,798.8673 ns | 277.4382 ns | 15.2073 ns | 2,791.2215 ns | 1.000 |    0.00 | 1.0071 | 0.0076 |    8442 B |        1.00 |
| NewTransit_WriteJson    |     0.0042 ns |   0.0077 ns |  0.0004 ns |     0.0043 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |
| OldTransit_WriteMsgPack |            NA |          NA |         NA |            NA |     ? |       ? |     NA |     NA |        NA |           ? |
| NewTransit_WriteMsgPack |     0.0016 ns |   0.0520 ns |  0.0029 ns |     0.0000 ns | 0.000 |    0.00 |      - |      - |         - |        0.00 |

Benchmarks with issues:
  WriteBenchmarks.OldTransit_WriteMsgPack: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
