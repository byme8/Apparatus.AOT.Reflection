using System;
using System.Reflection;
using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests
{
    public class KeyOfTests
    {
        [Fact]
        public async Task WorksWithCorrectProperty()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var property = new User().GetProperties()[""FirstName""];");

            var assembly = await project.CompileToRealAssembly();
        }
        
        [Fact]
        public async Task FailedWithWrongProperty()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var property = new User().GetProperties()[""Test""];");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }
        
        [Fact]
        public async Task WorkWithCorrectNameOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var property = new User().GetProperties()[nameof(User.FirstName)];");

            await project.CompileToRealAssembly();
        }
        
        [Fact]
        public async Task FailedWithWrongNameOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var property = new User().GetProperties()[nameof(Program.DontCall)];");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }
        
        [Fact]
        public async Task FailedWithWrongConstantOf()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"
                        const string propertyName = ""Test"";
                        var property = new User().GetProperties()[propertyName];
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }
        
        [Fact]
        public async Task FailedWithWrongPropertyNameInMethodCall()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
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
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
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
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
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
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"
                        GetIt(user, nameof(Program));
                        
                        IPropertyInfo GetIt<T>(T value, KeyOf<T> property)
                        {
                            return value.GetProperties()[property];
                        }
                    ");

            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }
    }
}