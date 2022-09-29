using System;
using System.Collections.Generic;
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{
    public static class AOTReflectionExtensions
    {
        public static IReadOnlyDictionary<KeyOf<TValue>, IPropertyInfo> GetProperties<TValue>(this TValue value) =>
            AOTReflection.GetProperties<TValue>();

        public static IEnumValueInfo<TEnum> GetEnumValueInfo<TEnum>(this TEnum value)
            where TEnum : Enum
        {
            var data = EnumMetadataStore<TEnum>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TEnum).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' to bootstrap it.");
            }

            return data.Value[value];
        }
    }
}