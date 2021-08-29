using System;
using System.Collections.Generic;

namespace AttributesExtractor
{
    public static class MetadataStore<T>
    {
        public static Lazy<IReadOnlyDictionary<string, IPropertyInfo>> Data { get; set; }
    }

    public static class GenericHelper
    {
        public static void Bootstrap<T>()
        {
        }
    }

    public static class AttributesExtractorExtensions
    {
        public static IReadOnlyDictionary<string, IPropertyInfo> GetProperties<TValue>(this TValue value)
        {
            var data = MetadataStore<TValue>.Data;
            if (data is null)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TValue).FullName}' is not registered. Use 'AttributesExtractor.GenericHelper.Bootstrap' or extension 'GetProperties' to bootstrap it.");
                return null;
            }

            return data.Value;
        }
    }
}