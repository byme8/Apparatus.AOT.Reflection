using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Core.Stores;
using Apparatus.AOT.Reflection.Playground;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests
{
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
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName", new Attribute[] { new RequiredAttribute(), }),
                new PropertyInfo<User, string>("LastName", new Attribute[] { new RequiredAttribute(), }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var attributes = user.GetProperties();");

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries.SequenceEqual(entries!));
        }

        [Fact]
        public async Task ExtractionFromClassWithArguments()
        {
            var expectedEntries = new[]
            {
                new PropertyInfo<User, string>("FirstName",
                    new Attribute[]
                    {
                        new RequiredAttribute(),
                        new DescriptionAttribute("Some first name")
                    }),
                new PropertyInfo<User, string>("LastName", new Attribute[] { new RequiredAttribute(), }),
            };

            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync(
                    (TestProject.Project.Name, "Program.cs", "// place to replace 1",
                        @"var attributes = user.GetProperties();"),
                    (TestProject.Core.Name, "User.cs", "// place to replace 2",
                        @"[System.ComponentModel.Description(""Some first name"")]"));

            var entries = await project.ExecutePropertiesTest();

            Assert.True(expectedEntries
                .SequenceEqual(entries!));
        }

        [Fact]
        public async Task GetterAndSettersWorks()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var attributes = user.GetProperties();");

            var user = new User
            {
                FirstName = "Jon",
                LastName = "Smith",
            };

            var entries = await project.ExecutePropertiesTest(user);

            var firstNameEntry = entries.First(o => o.Name == nameof(User.FirstName));
            var success = firstNameEntry.TryGetValue(user, out var value);

            Assert.True(success);
            Assert.Equal(user.FirstName, value);

            var newName = "Marry";
            success = firstNameEntry.TrySetValue(user, newName);

            Assert.True(success);
            Assert.Equal(user.FirstName, newName);
        }

        [Fact]
        public async Task ExecuteGetterOnWrongType()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var attributes = user.GetProperties();");

            var user = new User
            {
                FirstName = "Jon",
                LastName = "Smith",
            };

            var entries = await project.ExecutePropertiesTest(user);

            var firstNameEntry = entries.First(o => o.Name == nameof(User.FirstName));
            var success = firstNameEntry.TryGetValue(project, out var value);

            Assert.False(success);
        }

        [Fact]
        public async Task ExecuteSetterOnWrongType()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    @"var attributes = user.GetProperties();");

            var user = new User
            {
                FirstName = "Jon",
                LastName = "Smith",
            };

            var entries = await project.ExecutePropertiesTest(user);

            var firstNameEntry = entries.First(o => o.Name == nameof(User.FirstName));
            var success = firstNameEntry.TrySetValue(project, "Marry");

            Assert.False(success);
        }

        [Fact]
        public async Task WorksWithGenerics()
        {
            var assembly = await TestProject.Project.CompileToRealAssembly();
            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetUserInfo", BindingFlags.Static | BindingFlags.Public)!;

            var properties = methodInfo
                .Invoke(null, null);

            Assert.NotNull(properties);
        }
        
        [Fact]
        public async Task ExceptionFiredWhenTypeIsNotBootstrapped()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "var attributes = user.GetProperties();", "");
            var assembly = await project.CompileToRealAssembly();

            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetUserInfo", BindingFlags.Static | BindingFlags.Public)!;

            Assert.Throws<TargetInvocationException>(() =>
            {
                methodInfo.Invoke(null, null);
                TypedMetadataStore.Types.Clear();
                return methodInfo.Invoke(null, null);
            });
        }
        [Fact]
        public async Task PropertiesWithOverrideAndNewHandledProperly()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs", "// place to replace 1",
                    "var attributes = new Admin().GetProperties();");

            var assembly = await project.CompileToRealAssembly();

            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetUserInfo", BindingFlags.Static | BindingFlags.Public)!;

            var properties = methodInfo
                .Invoke(null, null);

            Assert.NotNull(properties);
        }

        [Fact]
        public async Task PrivateClassesHandledProperly()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"),
                    ("var user = new User();", "var user = new PrivateUser();"),
                    ("// place to replace 1", " var attributes = user.GetProperties();"));

            var assembly = await project.CompileToRealAssembly();

            var privateUser = assembly.GetType("Apparatus.AOT.Reflection.Playground.PrivateUser");
            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(privateUser);

            var properties = methodInfo
                .Invoke(null, new[] { Activator.CreateInstance(privateUser) });

            Assert.NotNull(properties);
        }

        [Fact]
        public async Task KeyOfTryParseDetected()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"),
                    ("var user = new User(); // 1", @"var success = KeyOf<PrivateUser>.TryParse(""FirstName"", out var key);"));

            var assembly = await project.CompileToRealAssembly();

            var privateUser = assembly.GetType("Apparatus.AOT.Reflection.Playground.PrivateUser");
            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(privateUser);

            var properties = methodInfo
                .Invoke(null, new[] { Activator.CreateInstance(privateUser) });

            Assert.NotNull(properties);
        }

        [Fact]
        public async Task KeyOfInMethodDeclarationDetected()
        {
            var project = await TestProject.Project
                .ReplacePartOfDocumentAsync("Program.cs",
                    ("// place to replace 0", "class PrivateUser { public string FirstName { get; set; } }"),
                    ("private static void DontCall()", @"private static void DontCall(Apparatus.AOT.Reflection.KeyOf<PrivateUser> key)"));

            var assembly = await project.CompileToRealAssembly();

            var privateUser = assembly.GetType("Apparatus.AOT.Reflection.Playground.PrivateUser");
            var methodInfo = assembly
                .GetType("Apparatus.AOT.Reflection.Playground.Program")!
                .GetMethod("GetInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(privateUser);

            var properties = methodInfo
                .Invoke(null, new[] { Activator.CreateInstance(privateUser) });

            Assert.NotNull(properties);
        }
    }
}