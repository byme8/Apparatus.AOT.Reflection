``` ini

BenchmarkDotNet=v0.13.1, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.404
  [Host]     : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT
  DefaultJob : .NET 6.0.12 (6.0.1222.56807), Arm64 RyuJIT


```
|        Method |      Mean |    Error |   StdDev |  Gen 0 | Allocated |
|-------------- |----------:|---------:|---------:|-------:|----------:|
|    Reflection | 651.09 ns | 0.445 ns | 0.395 ns | 0.0877 |     184 B |
| AOTReflection |  35.00 ns | 0.036 ns | 0.032 ns | 0.0114 |      24 B |
