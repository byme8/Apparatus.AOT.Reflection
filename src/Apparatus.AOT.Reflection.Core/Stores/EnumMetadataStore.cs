using System;
using System.Collections.Generic;

namespace Apparatus.AOT.Reflection.Core.Stores
{
    public static class EnumMetadataStore<T>
        where T : Enum
    {
        public static Lazy<IReadOnlyDictionary<T, IEnumValueInfo<T>>> Data { get; set; }
    }
}