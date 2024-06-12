using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;

namespace Apparatus.AOT.Reflection.Tests;

[UsesVerify]
public class AOTReflectionEnumsTests : Test
{
    [Fact]
    public async Task GetEnumValueInfoWorks()
    {
        var project = TestProject.Project;
        var result = await project.ExecuteTest("return UserKind.User.GetEnumValueInfo();");

        await Verify(result);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetAndDefaultEnumFromIntWorks(int value)
    {
        var project = TestProject.Project;
        var result = await project.ExecuteTest($"return EnumHelper.GetOrDefault({value}, UserKind.User);");

        await Verify(result)
            .UseParameters(value);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetEnumFromIntWorks(int value)
    {
        var project = TestProject.Project;
        var result = await project.ExecuteTest($"return EnumHelper.Get<UserKind>({value});");

        await Verify(result)
            .UseParameters(value);
    }

    [Fact]
    public async Task GetEnumValueInfoWorksWithAttributes()
    {
        var project = TestProject.Project;
        var result = await project.ExecuteTest("return UserKind.Admin.GetEnumValueInfo();");

        await Verify(result);
    }
    
    [Fact]
    public async Task PrivateEnumHandledProperly()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "enum TestEnum { Value2 = 2, Value3 = 3 }"));

        var expected = await project.ExecuteTest("return TestEnum.Value2.GetEnumValueInfo();");

        await Verify(expected);
    }
    
    [Fact]
    public async Task GetEnumInfoWorks()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "enum TestEnum { Value2 = 2, Value3 = 3 }"));

        var expected = await project.ExecuteTest("return EnumHelper.GetEnumInfo<TestEnum>();");

        await Verify(expected);
    }
    
    [Fact]
    public async Task ToIntWorks()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "enum TestEnum { Value2 = 2, Value3 = 3 }"));

        var expected = await project.ExecuteTest("return TestEnum.Value2.ToInt();");

        await Verify(expected);
    }
    
    [Fact]
    public async Task FromIntWorks()
    {
        var project = await TestProject.Project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("// place to replace 0", "enum TestEnum { Value2 = 2, Value3 = 3 }"));

        var expected = await project.ExecuteTest("return EnumHelper.FromInt<TestEnum>(2);");

        await Verify(expected);
    }
}