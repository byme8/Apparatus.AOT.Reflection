using System.Threading.Tasks;
using AttributesExtractor.Tests.Data;
using AttributesExtractor.Tests.Utils;
using Xunit;

namespace AttributesExtractor.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task CompiledWithoutErrors()
        {
            var project = TestProject.Project;
            var assembly = await project.CompileToRealAssembly();
            var type = assembly.GetType("AttributesExtractor.AttributesExtractorExtensions");
            
            Assert.NotNull(type);
        }
    }
}
