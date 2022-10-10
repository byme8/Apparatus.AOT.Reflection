``` ini

BenchmarkDotNet=v0.13.1, OS=macOS Monterey 12.5.1 (21G83) [Darwin 21.6.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.400
  [Host]     : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT
  DefaultJob : .NET 6.0.8 (6.0.822.36306), Arm64 RyuJIT


```
| Method |       Mean |     Error |    StdDev |
|------- |-----------:|----------:|----------:|
| Covert | 19.1729 ns | 0.0169 ns | 0.0150 ns |
|   Cast |  7.9034 ns | 0.0298 ns | 0.0279 ns |
| AOTOld |  5.2556 ns | 0.0101 ns | 0.0094 ns |
| AOTNew |  0.8851 ns | 0.0036 ns | 0.0034 ns |
