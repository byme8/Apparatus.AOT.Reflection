using System.Reflection;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;

namespace Apparatus.AOT.Reflection.Tests
{
    [UsesVerify]
    public class AOTReflectionPropertiesTests : Test
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
            var entries = await TestProject.Project.ExecuteTest("return new User().GetProperties();");

            await Verify(entries);
        }

        [Fact]
        public async Task ExtractionFromClassWithArguments()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name,
                        "User.cs", "// place to replace 2",
                        @"[System.ComponentModel.Description(""Some first name"")]"));

            var expected = await project.ExecuteTest("return new User().GetProperties().Values.First().Attributes.Last();");

            await Verify(expected);
        }

        [Fact]
        public async Task GetterAndSettersWorks()
        {
            var expected = await TestProject.Project
                .ExecuteTest(@"
            var user = new User
            {
                FirstName = ""Jon"",
                LastName = ""Smith"",
            };
            var entries = user.GetProperties();

            var firstNameEntry = entries.First(o => o.Key == nameof(User.FirstName)).Value;
            var success = firstNameEntry.TryGetValue(user, out var value);
            var newName = ""Marry"";
            success = firstNameEntry.TrySetValue(user, newName);

            return user;
");

            await Verify(expected);

        }

        [Fact]
        public async Task ExecuteGetterOnWrongType()
        {

            var expected = await TestProject.Project
                .ExecuteTest(@"
            var user = new User
            {
                FirstName = ""Jon"",
                LastName = ""Smith"",
            };
            var entries = user.GetProperties();

            var firstNameEntry = entries.First(o => o.Key == nameof(User.FirstName)).Value;

            var wrongType = entries;
            var success = firstNameEntry.TryGetValue(wrongType, out var value);

            return success;
");

            await Verify(expected);
        }

        [Fact]
        public async Task ExecuteSetterOnWrongType()
        {
            var expected = await TestProject.Project
                .ExecuteTest(@"
            var user = new User
            {
                FirstName = ""Jon"",
                LastName = ""Smith"",
            };
            var entries = user.GetProperties();

            var firstNameEntry = entries.First(o => o.Key == nameof(User.FirstName)).Value;

            var wrongType = entries;
            var success = firstNameEntry.TrySetValue(wrongType, ""Merry"");

            return success;
");

            await Verify(expected);
        }

        [Fact]
        public async Task ExceptionFiredWhenTypeIsNotBootstrapped()
        {
            await Assert.ThrowsAsync<TargetInvocationException>(async () =>
            {
                await TestProject.Project.ExecuteTest(@"
                    var user = new User();
                    TypedMetadataStore.Types.Clear();
                    return user.GetProperties();"
                );
            });
        }

        [Fact]
        public async Task PropertiesWithOverrideAndNewHandledProperly()
        {
            var expected = await TestProject.Project
                .ExecuteTest("return new Admin().GetProperties();");


            await Verify(expected);
        }

        [Fact]
        public async Task PrivateClassesHandledProperly()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"));


            var expected = await project.ExecuteTest("return new PrivateUser().GetProperties();");

            await Verify(expected);
        }

        [Fact]
        public async Task KeyOfTryParseDetected()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"));
            
            var expected = await project.ExecuteTest(@"return KeyOf<PrivateUser>.TryParse(""FirstName"", out var key);");

            await Verify(expected);
        }
        
        [Fact]
        public async Task KeyOfInMethodDeclarationDetected()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"));
        
            var expected = await project.ExecuteTest(@"
                return TypedMetadataStore.Types.Select(o => o.Key.Name);

                void DontCall(Apparatus.AOT.Reflection.KeyOf<PrivateUser> key)
                {
                }
");

            await Verify(expected);
        }
    }
}