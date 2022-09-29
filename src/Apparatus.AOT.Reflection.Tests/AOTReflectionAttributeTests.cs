using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;
namespace Apparatus.AOT.Reflection.Tests;

public class AOTReflectionAttributeTests
{
    [Fact]
    public async Task EnumWithAttributeDetected()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "[AOTReflection]public enum TestEnum { Value1, Value2 }"),
            ("// place to replace 1", "EnumMetadataStore<TestEnum>.Data.Value.First();"));

        await project.Validate();
    } 
}