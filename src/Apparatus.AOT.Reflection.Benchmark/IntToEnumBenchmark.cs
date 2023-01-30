using System;
using System.Runtime.CompilerServices;
using Apparatus.AOT.Reflection.Playground;
using BenchmarkDotNet.Attributes;

namespace Apparatus.AOT.Reflection.Benchmark;

[MemoryDiagnoser]
public class IntToEnumBenchmark
{
    [Benchmark]
    public UserKind Cast() => (UserKind)1;
    
    [Benchmark]
    public UserKind UnsafeAs()
    {
        var enumValue = 1;
        return Cast<UserKind>(enumValue);

        TEnum Cast<TEnum>(int value)
            where TEnum : Enum
        {
            return Unsafe.As<int, TEnum>(ref value);
        }
    }

    [Benchmark]
    public UserKind AOT() => EnumHelper.FromInt<UserKind>(1);
}