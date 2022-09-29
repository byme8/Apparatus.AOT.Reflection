using System;

namespace Apparatus.AOT.Reflection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Parameter)]
    public class AOTReflectionAttribute : Attribute
    {
        
    }
}