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
| OldTransit_WriteCache | 595.8 μs | 43.13 μs | 2.36 μs |  1.00 | 160.1563 | 73.2422 |      - | 1312.52 KB |        1.00 |
| NewTransit_WriteCache | 321.7 μs | 15.14 μs | 0.83 μs |  0.54 | 116.6992 | 51.7578 |      - |  956.68 KB |        0.73 |
| OldTransit_ReadCache  | 502.4 μs | 57.54 μs | 3.15 μs |  0.84 | 151.3672 | 28.3203 | 3.9063 | 1240.54 KB |        0.95 |
| NewTransit_ReadCache  | 227.1 μs |  2.98 μs | 0.16 μs |  0.38 |  75.4395 | 29.2969 | 1.9531 |  618.73 KB |        0.47 |
