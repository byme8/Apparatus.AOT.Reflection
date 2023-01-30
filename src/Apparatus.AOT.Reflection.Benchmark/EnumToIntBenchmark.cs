using System;
using System.Runtime.CompilerServices;
using Apparatus.AOT.Reflection.Playground;
using BenchmarkDotNet.Attributes;

namespace Apparatus.AOT.Reflection.Benchmark;

[MemoryDiagnoser]
public class EnumToIntBenchmark
{
    [Benchmark]
    public int Covert() => Convert.ToInt32(UserKind.Admin);

    [Benchmark]
    public int Cast() => (int)UserKind.Admin;

    [Benchmark]
    // enum => object => int - required because it works with generics
    // otherwise it will be enum => int
    public int CastGenericCompatible()
    {
        return Cast(UserKind.Admin);

        int Cast<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return (int)(object)value!;
        }
    }

    [Benchmark]
    public int UnsafeAs()
    {
        var enumValue = UserKind.Admin;
        return Cast(enumValue);

        int Cast<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return Unsafe.As<TEnum, int>(ref value);
        }
    }

    [Benchmark]
    public int AOTOld() => UserKind.Admin.GetEnumValueInfo().RawValue;

    [Benchmark]
    public int AOTNew() => UserKind.Admin.ToInt();
}