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
| OldTransit_WriteCache | 594.6 μs | 46.16 μs | 2.53 μs |  1.00 | 160.1563 | 74.2188 |      - | 1312.59 KB |        1.00 |
| NewTransit_WriteCache | 337.2 μs | 19.79 μs | 1.08 μs |  0.57 | 122.0703 | 53.7109 |      - |  999.11 KB |        0.76 |
| OldTransit_ReadCache  | 511.7 μs | 50.92 μs | 2.79 μs |  0.86 | 151.3672 | 28.3203 | 2.9297 | 1240.53 KB |        0.95 |
| NewTransit_ReadCache  | 227.2 μs | 55.54 μs | 3.04 μs |  0.38 |  75.4395 | 30.0293 | 1.7090 |  618.73 KB |        0.47 |
