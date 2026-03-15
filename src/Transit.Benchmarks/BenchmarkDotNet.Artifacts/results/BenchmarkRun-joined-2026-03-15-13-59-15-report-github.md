```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=Dry  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  RunStrategy=ColdStart  UnrollFactor=1  
WarmupCount=1  

```
| Type            | Method                      | Mean        | Error        | StdDev      | Median       | Ratio | RatioSD | Allocated  | Alloc Ratio |
|---------------- |---------------------------- |------------:|-------------:|------------:|-------------:|------:|--------:|-----------:|------------:|
| CacheBenchmarks | OldTransit_WriteCache       | 2,988.12 μs |  1,448.73 μs |    79.41 μs | 2,986.417 μs | 1.000 |    0.00 |    1318 KB |        1.00 |
| ReadBenchmarks  | OldTransit_ReadJson         |    35.78 μs |    590.52 μs |    32.37 μs |    22.542 μs | 0.012 |    0.01 |   28.66 KB |        0.02 |
| WriteBenchmarks | OldTransit_WriteJson        |    60.35 μs |    772.95 μs |    42.37 μs |    45.000 μs | 0.020 |    0.01 |   14.98 KB |        0.01 |
| CacheBenchmarks | NewTransit_WriteCache       | 1,650.18 μs |  2,521.45 μs |   138.21 μs | 1,590.459 μs | 0.552 |    0.04 | 1009.81 KB |        0.77 |
| ReadBenchmarks  | NewTransit_ReadJson         |    30.97 μs |    642.57 μs |    35.22 μs |    13.167 μs | 0.010 |    0.01 |   16.15 KB |        0.01 |
| WriteBenchmarks | NewTransit_WriteJson        |    40.57 μs |    756.75 μs |    41.48 μs |    21.041 μs | 0.013 |    0.01 |   73.84 KB |        0.06 |
| CacheBenchmarks | OldTransit_ReadCache        | 3,672.92 μs | 48,715.45 μs | 2,670.26 μs | 2,171.584 μs | 1.216 |    0.85 | 1243.87 KB |        0.94 |
| ReadBenchmarks  | OldTransit_ReadJsonVerbose  |    32.60 μs |    549.28 μs |    30.11 μs |    19.458 μs | 0.011 |    0.01 |   28.66 KB |        0.02 |
| WriteBenchmarks | OldTransit_WriteJsonVerbose |   254.68 μs |  6,914.47 μs |   379.01 μs |    42.583 μs | 0.083 |    0.12 |   16.31 KB |        0.01 |
| CacheBenchmarks | NewTransit_ReadCache        | 1,983.68 μs | 30,863.17 μs | 1,691.71 μs | 1,014.666 μs | 0.655 |    0.54 |   673.3 KB |        0.51 |
| ReadBenchmarks  | NewTransit_ReadJsonVerbose  |    30.35 μs |    789.13 μs |    43.25 μs |     7.209 μs | 0.010 |    0.01 |   16.15 KB |        0.01 |
| WriteBenchmarks | NewTransit_WriteJsonVerbose |    93.99 μs |  2,371.57 μs |   129.99 μs |    20.167 μs | 0.031 |    0.04 |   13.44 KB |        0.01 |
