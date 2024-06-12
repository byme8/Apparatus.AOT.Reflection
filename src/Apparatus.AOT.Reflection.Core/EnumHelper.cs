using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{
    public static class EnumHelper
    {
        public static IEnumerable<IEnumValueInfo<TEnum>> GetEnumInfo<TEnum>()
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
            }

            return data.Value.Values;
        }

        public static bool IsDefined<TEnum>(TEnum value)
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
            }

            return data.Value.ContainsKey(value);
        }

        public static string GetName<TEnum>(TEnum value)
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
            }

            return data.Value[value].Name;
        }

        public static IEnumValueInfo<TEnum> CreateOrDefault<TEnum>(int value, TEnum @default)
            where TEnum : Enum
        {
            var enumValue = FromInt<TEnum>(value);
            return CreateOrDefault(enumValue, @default);
        }

        public static IEnumValueInfo<TEnum> CreateOrDefault<TEnum>(TEnum value, TEnum @default)
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
            }

            if (IsDefined(value))
            {
                return data.Value[value];
            }

            return data.Value[@default];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum FromInt<TEnum>(int value)
            where TEnum : Enum
        {
            return Unsafe.As<int, TEnum>(ref value);
        }
    }
}