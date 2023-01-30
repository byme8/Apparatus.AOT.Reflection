``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.404
  [Host]     : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT
  DefaultJob : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT


```
|              Method |       Mean |     Error |    StdDev |  Gen 0 | Allocated |
|-------------------- |-----------:|----------:|----------:|-------:|----------:|
|        GetValuesAOT |   7.980 ns | 0.0150 ns | 0.0140 ns |      - |         - |
| GetValuesReflection | 861.670 ns | 1.0458 ns | 0.9782 ns | 0.1297 |     272 B |
