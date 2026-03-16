```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                | Mean     | Error    | StdDev  | Ratio | Gen0     | Gen1    | Gen2   | Allocated  | Alloc Ratio |
|---------------------- |---------:|---------:|--------:|------:|---------:|--------:|-------:|-----------:|------------:|
| OldTransit_WriteCache | 572.6 μs | 26.38 μs | 1.45 μs |  1.00 | 160.1563 | 73.2422 |      - | 1312.58 KB |        1.00 |
| NewTransit_WriteCache | 320.5 μs | 20.17 μs | 1.11 μs |  0.56 | 116.6992 | 51.7578 |      - |  956.68 KB |        0.73 |
| OldTransit_ReadCache  | 502.1 μs |  3.37 μs | 0.18 μs |  0.88 | 151.3672 | 27.3438 | 3.9063 | 1240.54 KB |        0.95 |
| NewTransit_ReadCache  | 228.4 μs | 16.10 μs | 0.88 μs |  0.40 |  75.4395 | 29.2969 | 2.1973 |  618.73 KB |        0.47 |
