```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  ShortRun : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Type            | Method                  | Mean            | Error          | StdDev      | Median          | Ratio | RatioSD | Gen0     | Gen1    | Allocated | Alloc Ratio |
|---------------- |------------------------ |----------------:|---------------:|------------:|----------------:|------:|--------:|---------:|--------:|----------:|------------:|
| CacheBenchmarks | OldTransit_WriteCache   | 579,505.4258 ns | 14,542.6456 ns | 797.1315 ns | 579,500.4072 ns | 1.000 |    0.00 | 160.1563 | 73.2422 | 1343953 B |        1.00 |
| ReadBenchmarks  | OldTransit_ReadJson     |              NA |             NA |          NA |              NA |     ? |       ? |       NA |      NA |        NA |           ? |
| WriteBenchmarks | OldTransit_WriteJson    |   2,806.7521 ns |    107.4917 ns |   5.8920 ns |   2,809.6298 ns | 0.005 |    0.00 |   1.0071 |  0.0076 |    8442 B |        0.01 |
| CacheBenchmarks | NewTransit_WriteCache   |       0.0009 ns |      0.0121 ns |   0.0007 ns |       0.0012 ns | 0.000 |    0.00 |        - |       - |         - |        0.00 |
| ReadBenchmarks  | NewTransit_ReadJson     |              NA |             NA |          NA |              NA |     ? |       ? |       NA |      NA |        NA |           ? |
| WriteBenchmarks | NewTransit_WriteJson    |       0.0058 ns |      0.1763 ns |   0.0097 ns |       0.0003 ns | 0.000 |    0.00 |        - |       - |         - |        0.00 |
| CacheBenchmarks | OldTransit_ReadCache    | 462,725.0369 ns | 11,452.1928 ns | 627.7333 ns | 462,421.1836 ns | 0.798 |    0.00 | 151.8555 | 62.5000 | 1270288 B |        0.95 |
| ReadBenchmarks  | OldTransit_ReadMsgPack  |              NA |             NA |          NA |              NA |     ? |       ? |       NA |      NA |        NA |           ? |
| WriteBenchmarks | OldTransit_WriteMsgPack |              NA |             NA |          NA |              NA |     ? |       ? |       NA |      NA |        NA |           ? |
| CacheBenchmarks | NewTransit_ReadCache    |       0.0000 ns |      0.0000 ns |   0.0000 ns |       0.0000 ns | 0.000 |    0.00 |        - |       - |         - |        0.00 |
| ReadBenchmarks  | NewTransit_ReadMsgPack  |              NA |             NA |          NA |              NA |     ? |       ? |       NA |      NA |        NA |           ? |
| WriteBenchmarks | NewTransit_WriteMsgPack |       0.0019 ns |      0.0596 ns |   0.0033 ns |       0.0000 ns | 0.000 |    0.00 |        - |       - |         - |        0.00 |

Benchmarks with issues:
  ReadBenchmarks.OldTransit_ReadJson: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  ReadBenchmarks.NewTransit_ReadJson: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  ReadBenchmarks.OldTransit_ReadMsgPack: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  WriteBenchmarks.OldTransit_WriteMsgPack: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
  ReadBenchmarks.NewTransit_ReadMsgPack: ShortRun(IterationCount=3, LaunchCount=1, WarmupCount=3)
