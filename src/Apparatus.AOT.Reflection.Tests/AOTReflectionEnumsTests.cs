using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Apparatus.AOT.Reflection.Playground;
using Apparatus.AOT.Reflection.Tests.Data;
using Apparatus.AOT.Reflection.Tests.Utils;
using Xunit;

namespace Apparatus.AOT.Reflection.Tests
{
    public class AOTReflectionEnumsTests : Test
    {
        [Fact]
        public async Task GetEnumValueInfoWorks()
        {
            var expected = new EnumValueInfo<UserKind>("User", (int)UserKind.User, UserKind.User, new Attribute[0]);

            var assembly = await TestProject.Project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Playground.Program");
            Assert.NotNull(extension);

            var method = extension
                .GetMethod("GetEnumValueInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(typeof(UserKind))
                .CreateDelegate<Func<UserKind, IEnumValueInfo<UserKind>>>();
            Assert.NotNull(method);

            var entry = method.Invoke(UserKind.User);
            Assert.Equal(expected, entry);
        }

        [Fact]
        public async Task GetEnumValueInfoWorksWithAttributes()
        {
            var expected = new EnumValueInfo<UserKind>("Admin", (int)UserKind.User, UserKind.Admin, new Attribute[] { new DescriptionAttribute("Admin user") });

            var assembly = await TestProject.Project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Playground.Program");
            Assert.NotNull(extension);

            var method = extension
                .GetMethod("GetEnumValueInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(typeof(UserKind))
                .CreateDelegate<Func<UserKind, IEnumValueInfo<UserKind>>>();
            Assert.NotNull(method);

            var entry = method.Invoke(UserKind.Admin);
            Assert.Equal(expected, entry);
        }

        [Fact]
        public async Task PrivateEnumHandledProperly()
        {
            var expected = new EnumValueInfo<UserKind>("Admin", (int)UserKind.Admin, UserKind.Admin, new Attribute[] { new DescriptionAttribute("Admin user") });

            var assembly = await TestProject.Project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Playground.Program");
            Assert.NotNull(extension);

            var method = extension
                .GetMethod("GetEnumValueInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(typeof(UserKind))
                .CreateDelegate<Func<UserKind, IEnumValueInfo<UserKind>>>();
            Assert.NotNull(method);

            var entry = method.Invoke(UserKind.Admin);
            Assert.Equal(expected, entry);
        }

        [Fact]
        public async Task GetEnumInfoWorks()
        {
            var expected = new[]
            {
                new EnumValueInfo<UserKind>("User", (int)UserKind.User, UserKind.User, new Attribute[0]),
                new EnumValueInfo<UserKind>("Admin", (int)UserKind.Admin, UserKind.Admin, new Attribute[] { new DescriptionAttribute("Admin user") })
            };

            var project = await TestProject.Project.ReplacePartOfDocumentAsync(
                "Program.cs",
                "var value = userKind.GetEnumValueInfo();",
                "EnumHelper.GetEnumInfo<UserKind>();");

            var assembly = await project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Playground.Program");
            Assert.NotNull(extension);

            var method = extension
                .GetMethod("GetEnumInfo", BindingFlags.Static | BindingFlags.Public)
                .MakeGenericMethod(typeof(UserKind))
                .CreateDelegate<Func<IEnumerable<IEnumValueInfo<UserKind>>>>();
            Assert.NotNull(method);

            var entries = method.Invoke();

            Assert.True(expected.SequenceEqual(entries));
        }
    }
}