using System;
using System.Reflection;
using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests
{
    [UsesVerify]
    public class KeyOfTests : Test
    {
        [Fact]
        public async Task WorksWithCorrectProperty()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"var property = new User().GetProperties()[""FirstName""];");

            var assembly = await project.CompileToRealAssembly();
        }

        [Fact]
        public async Task FailedWithWrongProperty()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"var property = new User().GetProperties()[""Test""];");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task WorkWithCorrectNameOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"var property = new User().GetProperties()[nameof(User.FirstName)];");

            await project.CompileToRealAssembly();
        }

        [Fact]
        public async Task FailedWithWrongNameOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"var property = new User().GetProperties()[nameof(Program.DontCall)];");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task FailedWithWrongConstantOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        const string propertyName = ""Test"";
                        var property = new User().GetProperties()[propertyName];
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task WorksForGenericKeyOf()
        {
            var expected = await TestProject.Project.ExecuteTest(@"
                   return GetIt(new User(), ""FirstName"");
                        
                    IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                    {
                        return value.GetProperties()[property];
                    }");

            await Verify(expected);
        }

        [Fact]
        public async Task FailedWithWrongPropertyNameInMethodCall()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        GetIt(user, ""Test"");
                        
                        IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                        {
                            return value.GetProperties()[property];
                        }
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task FailedWithWrongConstInMethodCall()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        const string Name = ""Test"";
                        GetIt(user, Name);
                        
                        IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                        {
                            return value.GetProperties()[property];
                        }
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task FailedWithWrongNameofInMethodCall()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        GetIt(user, nameof(Program.DontCall));
                        
                        IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                        {
                            return value.GetProperties()[property];
                        }
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task FailedWithWrongShortNameofInMethodCall()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        GetIt(user, nameof(Program));
                        
                        IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                        {
                            return value.GetProperties()[property];
                        }
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task FailedBecausePropertyNameIsVariable()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    "// main",
                    @"
                        var a = ""FirstName"";
                        var aa = user.GetProperties()[a];
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task WorksBecauseKeyOfSupplied()
        {
            var expected = await TestProject.Project.ExecuteTest(@"
                        var user = new User();
                        KeyOf<User>.TryParse(""FirstName"", out var propertyKey);
                        return user.GetProperties()[propertyKey];
                    ");

            await Verify(expected);
        }

        [Fact]
        public async Task WorksWithNonLocalConstants()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    ("// place to replace properties", @"public const string Name = ""FirstName"";"));

            var expected = await project.ExecuteTest(@"
                        var user = new User();
                        return user.GetProperties()[Name];
                    ");

            await Verify(expected);
        }

        [Fact]
        public async Task WorksKeyOfProperties()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    ("// place to replace properties", @"public static KeyOf<User> Name = ""FirstName"";"));

            var expected = await project.ExecuteTest(@"
                        var user = new User();
                        return user.GetProperties()[Name];
                    ");

            await Verify(expected);
        }

        [Fact]
        public async Task DontUseKeyOfConstructor()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    "// main",
                    @"var keyof = new KeyOf<User>(""FirstName"");");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task IgnoresGenericKeyOfT()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    "Program.cs",
                    "// main",
                    @"var genericType = typeof(KeyOf<>);");

            await project.CompileToRealAssembly();
        }
    }
}