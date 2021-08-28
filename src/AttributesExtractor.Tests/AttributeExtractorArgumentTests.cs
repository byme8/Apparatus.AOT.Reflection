using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AttributesExtractor.Playground;
using AttributesExtractor.Tests.Data;
using AttributesExtractor.Tests.Utils;
using Xunit;

namespace AttributesExtractor.Tests
{
    public class AttributeExtractorArgumentTests
    {
        [Fact]
        public async Task IntWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new[]
                {
                    new AttributeData(typeof(RequiredAttribute)),
                    new AttributeData(typeof(TestAttribute), 1, 0, null, null, null),
                }),
                new PropertyInfo<User, string>("LastName", new[] { new AttributeData(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(@int: 1)]"));

            var entries = await project.ExecuteTest();

            Assert.True(expectedEntries.Select(TestExtensions.Stringify)
                .SequenceEqual(entries!.Select(TestExtensions.Stringify)));
        }

        [Fact]
        public async Task StringWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new[]
                {
                    new AttributeData(typeof(RequiredAttribute)),
                    new AttributeData(typeof(TestAttribute), 0, 0, "test", null, null),
                }),
                new PropertyInfo<User, string>("LastName", new[] { new AttributeData(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(text: ""test"")]"));

            var entries = await project.ExecuteTest();

            Assert.True(expectedEntries.Select(TestExtensions.Stringify)
                .SequenceEqual(entries!.Select(TestExtensions.Stringify)));
        }

        [Fact]
        public async Task StringArrayWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new[]
                {
                    new AttributeData(typeof(RequiredAttribute)),
                    new AttributeData(typeof(TestAttribute), 0, 0, null, new[] { "test", "test1" }, null),
                }),
                new PropertyInfo<User, string>("LastName", new[] { new AttributeData(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2",
                        @"[Test(textArray: new[] { ""test"", ""test1"" })]"));

            var entries = await project.ExecuteTest();

            Assert.True(expectedEntries.Select(TestExtensions.Stringify)
                .SequenceEqual(entries!.Select(TestExtensions.Stringify)));
        }

        [Fact]
        public async Task TypeWorks()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new[]
                {
                    new AttributeData(typeof(RequiredAttribute)),
                    new AttributeData(typeof(TestAttribute), 0, 0, null, null, typeof(int)),
                }),
                new PropertyInfo<User, string>("LastName", new[] { new AttributeData(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2", @"[Test(type: typeof(int))]"));

            var entries = await project.ExecuteTest();

            Assert.True(expectedEntries.Select(TestExtensions.Stringify)
                .SequenceEqual(entries!.Select(TestExtensions.Stringify)));
        }
    }
}