using System;

namespace Apparatus.AOT.Reflection.Playground
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class TestAttribute : Attribute
    {
        public TestAttribute(
            int @int = default,
            float @float = default,
            string text = default,
            string[] textArray = default,
            Type type = default)
        {
        }

        public TestAttribute(string text = default)
        {
        }
    }
}