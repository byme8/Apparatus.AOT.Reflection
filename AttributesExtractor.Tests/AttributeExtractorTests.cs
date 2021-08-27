using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AttributesExtractor.SourceGenerator;
using AttributesExtractor.Tests.Data;
using AttributesExtractor.Tests.Utils;
using Xunit;

namespace AttributesExtractor.Tests
{
    public class AttributeExtractorTests
    {
        [Fact]
        public async Task CompiledWithoutErrors()
        {
            var project = TestProject.Project;
            var assembly = await project.CompileToRealAssembly();
        }

        [Fact]
        public async Task BasicExtractionFromClass()
        {
            var expectedEntries = new[]
            {
                new Entry("FirstName", new[] { new Entry.EntryAttribute(typeof(RequiredAttribute)) }),
                new Entry("LastName", new[] { new Entry.EntryAttribute(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var attributes = user.GetAttributes();");

            var assembly = await project.CompileToRealAssembly();

            var extension = assembly.GetType("AttributesExtractor.AttributesExtractor_Playground_UserExtensions");
            var user = assembly.GetType("AttributesExtractor.Playground.User");
            Assert.NotNull(extension);
            Assert.NotNull(user);

            var method = extension.GetMethod("GetAttributes", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var entries = (Entry[])method.Invoke(null, new[] { Activator.CreateInstance(user) });

            Assert.True(expectedEntries.Select(Stringify).SequenceEqual(entries!.Select(Stringify)));
        }

        [Fact]
        public async Task ExtractionFromClassWithArguments()
        {
            var expectedEntries = new[]
            {
                new Entry("FirstName",
                    new[]
                    {
                        new Entry.EntryAttribute(typeof(RequiredAttribute)),
                        new Entry.EntryAttribute(typeof(DescriptionAttribute), "Some first name")
                    }),
                new Entry("LastName", new[] { new Entry.EntryAttribute(typeof(RequiredAttribute)) }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    ("// place to replace 1", @"var attributes = user.GetAttributes();"),
                    ("// place to replace 2", @"[System.ComponentModel.Description(""Some first name"")]"));

            var assembly = await project.CompileToRealAssembly();

            var extension = assembly.GetType("AttributesExtractor.AttributesExtractor_Playground_UserExtensions");
            var user = assembly.GetType("AttributesExtractor.Playground.User");
            Assert.NotNull(extension);
            Assert.NotNull(user);

            var method = extension.GetMethod("GetAttributes", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var entries = (Entry[])method.Invoke(null, new[] { Activator.CreateInstance(user) });

            Assert.True(expectedEntries.Select(Stringify).SequenceEqual(entries!.Select(Stringify)));
        }

        private string Stringify(Entry entry)
        {
            return $"{entry.PropertyName}{entry.Attributes.Select(o => $"{o.Type.FullName}{o.Parameters.Select(oo => oo.ToString()).Join()}").Join()}";
        }
    }
}