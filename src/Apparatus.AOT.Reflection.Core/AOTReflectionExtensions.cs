using System;
using System.Collections.Generic;

namespace Apparatus.AOT.Reflection
{
    public static class MetadataStore<T>
    {
        public static Lazy<IReadOnlyDictionary<string, IPropertyInfo>> Data { get; set; }
    }

    public static class EnumMetadataStore<T>
        where T : Enum
    {
        public static Lazy<IReadOnlyDictionary<T, IEnumValueInfo<T>>> Data { get; set; }
    }

    public static class GenericHelper
    {
        public static void Bootstrap<T>()
        {
        }
    }

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
                return null;
            }

            return data.Value.Values;
        }
    }

    public static class AOTReflectionExtensions
    {
        public static IReadOnlyDictionary<string, IPropertyInfo> GetProperties<TValue>(this TValue value)
        {
            var data = MetadataStore<TValue>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TValue).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' or extension 'GetProperties' to bootstrap it.");
                return null;
            }

            return data.Value;
        }

        public static IEnumValueInfo<TEnum> GetEnumValueInfo<TEnum>(this TEnum value)
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
                return null;
            }

            return data.Value[value];
        }
    }

    public static class AOTReflection 
    {
        public static IReadOnlyDictionary<string, IPropertyInfo> GetProperties<TValue>()
        {
            var data = MetadataStore<TValue>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TValue).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' or extension 'GetProperties' to bootstrap it.");
                return null;
            }

            return data.Value;
        }

    }
}