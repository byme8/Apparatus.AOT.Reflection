using System;
using Apparatus.AOT.Reflection.Core.Stores;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests.Utils
{
    [Collection("Sequential")]
    public class Test : IDisposable
    {
        public Test()
        {
            Config.Testing = true;
            TypedMetadataStore.Types.Clear();
        }

        public void Dispose()
        {
            Config.Testing = true;
            TypedMetadataStore.Types.Clear();
        }
    }
}