using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;

namespace Apparatus.AOT.Reflection.Tests
{
    [UsesVerify]
    public class AOTReflectionArgumentTests : Test
    {
        [Fact]
        public async Task IntWorks()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync((TestProject.Project.Name, "User.cs", "// place to replace 2", @"[Test(@int: 1)]"));

            var entries = await project.ExecuteTest("return new User().GetProperties().Values;");

            await Verify(entries);
        }

        [Fact]
        public async Task StringWorks()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync((TestProject.Project.Name, "User.cs", "// place to replace 2", @"[Test(text: ""test"")]"));

            var entries = await project.ExecuteTest("return new User().GetProperties().Values;");

            await Verify(entries);
        }

        [Fact]
        public async Task StringArrayWorks()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync((TestProject.Project.Name, "User.cs", "// place to replace 2", @"[Test(textArray: new[] { ""test"", ""test1"" })]"));

            var entries = await project.ExecuteTest("return new User().GetProperties().Values;");

            await Verify(entries);
        }

        [Fact]
        public async Task TypeWorks()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync((TestProject.Project.Name, "User.cs", "// place to replace 2", @"[Test(type: typeof(int))]"));

            var entries = await project.ExecuteTest("return new User().GetProperties().Values;");

            await Verify(entries);
        }
    }
}