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
| OldTransit_WriteCache | 574.6 μs | 26.02 μs | 1.43 μs |  1.00 | 160.1563 | 74.2188 |      - | 1312.39 KB |        1.00 |
| NewTransit_WriteCache | 338.9 μs | 18.63 μs | 1.02 μs |  0.59 | 116.6992 | 51.7578 |      - |  956.69 KB |        0.73 |
| OldTransit_ReadCache  | 497.5 μs | 28.48 μs | 1.56 μs |  0.87 | 143.5547 | 27.3438 | 5.8594 | 1178.04 KB |        0.90 |
| NewTransit_ReadCache  | 229.8 μs | 12.18 μs | 0.67 μs |  0.40 |  75.4395 | 27.0996 | 1.7090 |  618.73 KB |        0.47 |
