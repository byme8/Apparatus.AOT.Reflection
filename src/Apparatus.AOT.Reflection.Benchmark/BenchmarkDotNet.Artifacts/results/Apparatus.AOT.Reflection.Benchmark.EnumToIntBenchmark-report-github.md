``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.404
  [Host]     : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT
  DefaultJob : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT


```
|                Method |       Mean |     Error |    StdDev |     Median |  Gen 0 | Allocated |
|---------------------- |-----------:|----------:|----------:|-----------:|-------:|----------:|
|                Covert | 19.0282 ns | 0.0418 ns | 0.0371 ns | 19.0141 ns | 0.0229 |      48 B |
|                  Cast |  0.0007 ns | 0.0008 ns | 0.0007 ns |  0.0006 ns |      - |         - |
| CastGenericCompatible |  8.0456 ns | 0.0178 ns | 0.0158 ns |  8.0475 ns | 0.0115 |      24 B |
|              UnsafeAs |  0.0002 ns | 0.0005 ns | 0.0004 ns |  0.0001 ns |      - |         - |
|                AOTOld |  5.2976 ns | 0.0040 ns | 0.0037 ns |  5.2982 ns |      - |         - |
|                AOTNew |  0.0003 ns | 0.0006 ns | 0.0005 ns |  0.0000 ns |      - |         - |
