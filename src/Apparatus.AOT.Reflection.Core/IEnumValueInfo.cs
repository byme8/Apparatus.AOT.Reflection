#nullable enable
using System;

namespace Apparatus.AOT.Reflection
{
    public interface IEnumValueInfo<TEnum>
        where TEnum : Enum
    {
        string Name { get; }
        
        string? Description { get; }
        
        Attribute[] Attributes { get; }
        
        int RawValue { get; }
        
        TEnum Value { get; }
    }
}