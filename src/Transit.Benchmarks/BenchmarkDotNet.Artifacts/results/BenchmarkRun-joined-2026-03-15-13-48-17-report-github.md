```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=Dry  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  RunStrategy=ColdStart  UnrollFactor=1  
WarmupCount=1  

```
| Type            | Method                      | Mean         | Error         | StdDev       | Median        | Ratio | RatioSD | Allocated | Alloc Ratio |
|---------------- |---------------------------- |-------------:|--------------:|-------------:|--------------:|------:|--------:|----------:|------------:|
| CacheBenchmarks | OldTransit_WriteCache       | 2,994.847 μs |  1,776.426 μs |    97.372 μs | 3,005.7090 μs | 1.000 |    0.00 | 1349360 B |        1.00 |
| ReadBenchmarks  | OldTransit_ReadJson         |    31.945 μs |    431.347 μs |    23.644 μs |    19.5420 μs | 0.011 |    0.01 |   28672 B |        0.02 |
| WriteBenchmarks | OldTransit_WriteJson        |    57.986 μs |    849.007 μs |    46.537 μs |    37.7910 μs | 0.019 |    0.01 |   14672 B |        0.01 |
| CacheBenchmarks | NewTransit_WriteCache       |     7.653 μs |    233.265 μs |    12.786 μs |     0.2920 μs | 0.002 |    0.00 |         - |        0.00 |
| ReadBenchmarks  | OldTransit_ReadJsonVerbose  |    24.111 μs |    404.369 μs |    22.165 μs |    12.5420 μs | 0.008 |    0.01 |   28672 B |        0.02 |
| WriteBenchmarks | OldTransit_WriteJsonVerbose |   261.986 μs |  6,958.463 μs |   381.417 μs |    46.5000 μs | 0.085 |    0.12 |   16032 B |        0.01 |
| CacheBenchmarks | OldTransit_ReadCache        | 3,711.542 μs | 46,965.019 μs | 2,574.311 μs | 2,317.0000 μs | 1.226 |    0.81 | 1273720 B |        0.94 |
| ReadBenchmarks  | NewTransit_ReadJson         |     4.750 μs |    148.121 μs |     8.119 μs |     0.0830 μs | 0.002 |    0.00 |         - |        0.00 |
| WriteBenchmarks | NewTransit_WriteJson        |    12.903 μs |    404.430 μs |    22.168 μs |     0.2080 μs | 0.004 |    0.01 |         - |        0.00 |
| CacheBenchmarks | NewTransit_ReadCache        |     7.181 μs |    225.576 μs |    12.365 μs |     0.0840 μs | 0.002 |    0.00 |         - |        0.00 |
| ReadBenchmarks  | NewTransit_ReadJsonVerbose  |     5.125 μs |    160.629 μs |     8.805 μs |     0.0420 μs | 0.002 |    0.00 |         - |        0.00 |
| WriteBenchmarks | NewTransit_WriteJsonVerbose |     7.083 μs |    216.599 μs |    11.873 μs |     0.3330 μs | 0.002 |    0.00 |         - |        0.00 |
