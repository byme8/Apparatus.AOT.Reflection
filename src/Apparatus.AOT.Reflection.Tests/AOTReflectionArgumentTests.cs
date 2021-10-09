using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Playground;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests
{
    public class AOTReflectionArgumentTests : Test
    {
        [Fact]
        public async Task IntWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new Attribute[]
                {
                    new RequiredAttribute(),
                    new TestAttribute(1)
                }),
                new PropertyInfo<User, string>("LastName", new[] { new RequiredAttribute() }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(@int: 1)]"));

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries.SequenceEqual(entries!));
        }

        [Fact]
        public async Task StringWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new Attribute[]
                {
                    new RequiredAttribute(),
                    new TestAttribute("test")
                }),
                new PropertyInfo<User, string>("LastName", new[] { new RequiredAttribute(), }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(text: ""test"")]"));

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries.SequenceEqual(entries!));
        }

        [Fact]
        public async Task StringArrayWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new Attribute[]
                {
                    new RequiredAttribute(),
                    new TestAttribute(textArray: new[] { "test", "test1", })
                }),
                new PropertyInfo<User, string>("LastName", new[] { new RequiredAttribute(), }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2",
                        @"[Test(textArray: new[] { ""test"", ""test1"" })]"));

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries.SequenceEqual(entries!));
        }

        [Fact]
        public async Task TypeWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new Attribute[]
                {
                    new RequiredAttribute(),
                    new TestAttribute(type: typeof(int))
                }),
                new PropertyInfo<User, string>("LastName", new[] { new RequiredAttribute(), }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(type: typeof(int))]"));

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries.SequenceEqual(entries!));
        }
    }
}