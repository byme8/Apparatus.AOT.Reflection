using System;
using System.Collections.Generic;
using System.Linq;

namespace Apparatus.AOT.Reflection.Core.Stores
{
    public static class Config
    {
        public static bool Testing { get; set; }
    }

    public static class MetadataStore<T>
    {
        private static Dictionary<KeyOf<T>, IPropertyInfo> data;

        public static IReadOnlyDictionary<KeyOf<T>, IPropertyInfo> Data
        {
            get
            {
                if (data == null || Config.Testing)
                {
                    var keys = TypedMetadataStore.Types[typeof(T)];
                    data = keys.ToDictionary(o => o.Key as KeyOf<T>, o => o.Value);
                }

                return data;
            }
        }
    }

    public static class TypedMetadataStore
    {
        public static Dictionary<Type, IReadOnlyDictionary<IKeyOf, IPropertyInfo>> Types { get; }
            = new Dictionary<Type, IReadOnlyDictionary<IKeyOf, IPropertyInfo>>();
    }
}