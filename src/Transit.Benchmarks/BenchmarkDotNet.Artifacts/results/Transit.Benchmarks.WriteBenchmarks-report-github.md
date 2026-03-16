```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host] : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD

Job=ShortRun  Toolchain=InProcessNoEmitToolchain  IterationCount=3  
LaunchCount=1  WarmupCount=3  

```
| Method                      | Mean       | Error     | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------- |-----------:|----------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
| OldTransit_WriteJson        | 2,663.1 ns | 607.25 ns | 33.29 ns |  1.00 |    0.00 | 0.9842 | 0.0191 |   8.06 KB |        1.00 |
| NewTransit_WriteJson        |   838.2 ns | 129.16 ns |  7.08 ns |  0.31 |    0.01 | 0.4196 | 0.0038 |   3.43 KB |        0.43 |
| OldTransit_WriteJsonVerbose | 4,040.0 ns | 255.29 ns | 13.99 ns |  1.52 |    0.02 | 1.1749 | 0.0229 |   9.64 KB |        1.20 |
| NewTransit_WriteJsonVerbose | 2,181.1 ns | 139.97 ns |  7.67 ns |  0.82 |    0.01 | 1.1253 | 0.0343 |    9.2 KB |        1.14 |
| NewTransit_WriteArray       |   338.5 ns |  28.41 ns |  1.56 ns |  0.13 |    0.00 | 0.3018 | 0.0038 |   2.47 KB |        0.31 |
