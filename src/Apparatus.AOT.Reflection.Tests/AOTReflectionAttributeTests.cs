using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;

namespace Apparatus.AOT.Reflection.Tests;

[UsesVerify]
public class AOTReflectionAttributeTests
{
    [Fact]
    public async Task EnumWithAttributeDetected()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "[AOTReflection]public enum TestEnum { Value2 = 2, Value3 = 3 }"));

        var expected = await project.ExecuteTest("return EnumMetadataStore<TestEnum>.Data.Value;");

        await Verify(expected);
    }
    
    [Fact]
    public async Task ClassWithAttributeDetected()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "[AOTReflection]public class TestClass { public string Value { get; set; } }"));

        var expected = await project.ExecuteTest("return MetadataStore<TestClass>.Data;");

        await Verify(expected);
    }
}