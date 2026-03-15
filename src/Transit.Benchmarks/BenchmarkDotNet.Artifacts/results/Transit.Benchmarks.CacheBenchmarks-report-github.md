```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                | Mean            | Error           | StdDev        | Ratio | Gen0     | Gen1    | Allocated | Alloc Ratio |
|---------------------- |----------------:|----------------:|--------------:|------:|---------:|--------:|----------:|------------:|
| OldTransit_WriteCache | 583,161.6348 ns |  55,749.5508 ns | 3,055.8210 ns | 1.000 | 160.1563 | 73.2422 | 1343953 B |        1.00 |
| NewTransit_WriteCache |       0.0000 ns |       0.0000 ns |     0.0000 ns | 0.000 |        - |       - |         - |        0.00 |
| OldTransit_ReadCache  | 450,056.3083 ns | 136,974.4740 ns | 7,508.0331 ns | 0.772 | 144.0430 | 47.8516 | 1206288 B |        0.90 |
| NewTransit_ReadCache  |       0.0000 ns |       0.0000 ns |     0.0000 ns | 0.000 |        - |       - |         - |        0.00 |
