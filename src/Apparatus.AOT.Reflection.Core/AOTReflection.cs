using System;
using System.Collections.Generic;
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{
    public static class AOTReflection 
    {
        public static IReadOnlyDictionary<KeyOf<TValue>, IPropertyInfo> GetProperties<TValue>()
        {
            var data = MetadataStore<TValue>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TValue).FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' or extension 'GetProperties' to bootstrap it.");
            }

            return data;
        }
        
        public static IReadOnlyDictionary<IKeyOf, IPropertyInfo> GetProperties(Type type)
        {
            if (!TypedMetadataStore.Types.TryGetValue(type, out var data))
            {
                throw new InvalidOperationException(
                    $"Type '{type.FullName}' is not registered. Use 'Apparatus.AOT.Reflection.GenericHelper.Bootstrap' or extension 'GetProperties' to bootstrap it.");
            }

            return data;
        }
    }
}