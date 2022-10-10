using System;
namespace Apparatus.AOT.Reflection
{
    internal class ExceptionHelper
    {
        public static void ThrowTypeIsNotBootstrapped(Type type)
        {
            throw new InvalidOperationException(
                $"Type '{type.FullName}' is not registered. Use 'AOTReflection' attribute to bootstrap it.");
        }
    }
}