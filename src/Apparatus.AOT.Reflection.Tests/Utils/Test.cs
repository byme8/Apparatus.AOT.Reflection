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
#if DEBUG
            Config.Testing = true;
#endif
            TypedMetadataStore.Types.Clear();
        }

        public void Dispose()
        {
#if DEBUG
            Config.Testing = true;
#endif
            TypedMetadataStore.Types.Clear();
        }
    }
}