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
| OldTransit_WriteJson        | 2,697.0 ns | 542.53 ns | 29.74 ns |  1.00 |    0.00 | 0.9995 | 0.0191 |   8.18 KB |        1.00 |
| NewTransit_WriteJson        |   837.8 ns |  91.74 ns |  5.03 ns |  0.31 |    0.00 | 0.4177 | 0.0048 |   3.42 KB |        0.42 |
| OldTransit_WriteJsonVerbose | 3,847.9 ns | 365.33 ns | 20.02 ns |  1.43 |    0.02 | 1.1520 | 0.0229 |   9.46 KB |        1.16 |
| NewTransit_WriteJsonVerbose | 1,817.1 ns |  16.15 ns |  0.89 ns |  0.67 |    0.01 | 0.8965 | 0.0210 |   7.34 KB |        0.90 |
