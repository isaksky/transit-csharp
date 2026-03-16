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
| OldTransit_WriteCache | 594.4 μs | 46.43 μs | 2.55 μs |  1.00 | 160.1563 | 73.2422 |      - | 1312.52 KB |        1.00 |
| NewTransit_WriteCache | 325.9 μs | 14.23 μs | 0.78 μs |  0.55 | 109.3750 | 46.3867 |      - |  894.79 KB |        0.68 |
| OldTransit_ReadCache  | 507.3 μs | 22.83 μs | 1.25 μs |  0.85 | 151.3672 | 27.3438 | 3.9063 | 1240.54 KB |        0.95 |
| NewTransit_ReadCache  | 227.5 μs |  0.90 μs | 0.05 μs |  0.38 |  75.4395 | 29.2969 | 1.9531 |  618.73 KB |        0.47 |
