```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                      | Mean       | Error     | StdDev   | Ratio | Gen0   | Gen1   | Gen2   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|---------:|------:|-------:|-------:|-------:|----------:|------------:|
| OldTransit_WriteJson        | 2,724.6 ns | 311.98 ns | 17.10 ns |  1.00 | 0.9995 | 0.0191 |      - |   8.18 KB |        1.00 |
| NewTransit_WriteJson        |   456.5 ns |  65.67 ns |  3.60 ns |  0.17 | 0.1516 | 0.0005 |      - |   1.24 KB |        0.15 |
| OldTransit_WriteJsonVerbose | 3,875.6 ns | 249.21 ns | 13.66 ns |  1.42 | 1.1520 | 0.0229 |      - |   9.46 KB |        1.16 |
| NewTransit_WriteJsonVerbose | 2,227.2 ns | 156.50 ns |  8.58 ns |  0.82 | 0.8659 | 0.0114 | 0.0038 |    7.1 KB |        0.87 |
