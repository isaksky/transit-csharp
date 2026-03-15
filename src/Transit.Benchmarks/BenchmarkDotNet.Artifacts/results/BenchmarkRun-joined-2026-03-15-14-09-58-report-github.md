```

BenchmarkDotNet v0.13.12, macOS 15.7.4 (24G517) [Darwin 24.6.0]
Apple M3 Ultra, 1 CPU, 28 logical and 28 physical cores
.NET SDK 10.0.103
  [Host]     : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 10.0.3 (10.0.326.7603), Arm64 RyuJIT AdvSIMD


```
| Type            | Method                      | Mean         | Error       | StdDev      | Ratio | Gen0     | Gen1    | Gen2    | Allocated  | Alloc Ratio |
|---------------- |---------------------------- |-------------:|------------:|------------:|------:|---------:|--------:|--------:|-----------:|------------:|
| CacheBenchmarks | OldTransit_WriteCache       | 584,939.6 ns | 5,226.26 ns | 4,888.65 ns | 1.000 | 160.1563 | 73.2422 |       - | 1312.52 KB |       1.000 |
| ReadBenchmarks  | OldTransit_ReadJson         |   1,857.9 ns |    10.65 ns |     9.96 ns | 0.003 |   2.9316 |  0.2251 |       - |   23.99 KB |       0.018 |
| WriteBenchmarks | OldTransit_WriteJson        |   2,815.0 ns |     8.27 ns |     7.73 ns | 0.005 |   1.0071 |  0.0076 |       - |    8.24 KB |       0.006 |
| CacheBenchmarks | NewTransit_WriteCache       | 422,949.9 ns | 1,212.85 ns | 1,134.50 ns | 0.723 | 122.5586 | 52.2461 |       - | 1004.36 KB |       0.765 |
| ReadBenchmarks  | NewTransit_ReadJson         |     619.1 ns |     5.29 ns |     4.95 ns | 0.001 |   1.9646 |  0.1087 |       - |   16.09 KB |       0.012 |
| WriteBenchmarks | NewTransit_WriteJson        |   2,033.1 ns |     6.75 ns |     5.98 ns | 0.003 |   8.1940 |  1.6365 |       - |   67.17 KB |       0.051 |
| CacheBenchmarks | OldTransit_ReadCache        | 479,137.9 ns |   683.38 ns |   570.65 ns | 0.818 | 151.8555 | 62.5000 |       - | 1240.52 KB |       0.945 |
| ReadBenchmarks  | OldTransit_ReadJsonVerbose  |   1,863.5 ns |     7.71 ns |     7.21 ns | 0.003 |   2.9316 |  0.2251 |       - |   23.99 KB |       0.018 |
| WriteBenchmarks | OldTransit_WriteJsonVerbose |   4,386.5 ns |    18.25 ns |    17.07 ns | 0.007 |   1.1749 |  0.0229 |       - |    9.64 KB |       0.007 |
| CacheBenchmarks | NewTransit_ReadCache        | 304,960.3 ns | 1,234.86 ns | 1,094.67 ns | 0.521 |  83.0078 | 41.5039 | 41.5039 |   669.4 KB |       0.510 |
| ReadBenchmarks  | NewTransit_ReadJsonVerbose  |     618.3 ns |     4.98 ns |     4.41 ns | 0.001 |   1.9646 |  0.1087 |       - |   16.09 KB |       0.012 |
| WriteBenchmarks | NewTransit_WriteJsonVerbose |   1,766.5 ns |     3.73 ns |     3.49 ns | 0.003 |   0.8240 |  0.0172 |       - |    6.74 KB |       0.005 |
