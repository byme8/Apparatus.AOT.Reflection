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

        var expected = await project.ExecuteTest("return EnumHelper.GetEnumInfo<UserKind>();");

        await Verify(expected);
    }
}