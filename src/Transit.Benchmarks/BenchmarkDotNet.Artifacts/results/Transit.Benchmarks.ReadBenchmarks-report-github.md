```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD


```
| Method              | Mean       | Error   | StdDev  | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
|-------------------- |-----------:|--------:|--------:|------:|-------:|-------:|----------:|------------:|
| OldTransit_ReadJson | 1,808.5 ns | 6.61 ns | 5.16 ns |  1.00 | 2.9316 | 0.2251 |  23.99 KB |        1.00 |
| NewTransit_ReadJson |   626.2 ns | 8.43 ns | 7.47 ns |  0.35 | 1.9646 | 0.1087 |  16.09 KB |        0.67 |
