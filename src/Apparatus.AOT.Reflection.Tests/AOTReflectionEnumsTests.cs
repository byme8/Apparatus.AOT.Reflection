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
    public class AOTReflectionEnumsTests
    {
        [Fact]
        public async Task GetPropertiesOnEnumFails()
        {
            var project = await TestProject.Project.ReplacePartOfDocumentAsync(
                "Program.cs",
                "var enumValues = userKind.GetEnumValueInfo();",
                "var enumValues = userKind.GetProperties();");


            await Assert.ThrowsAsync<Exception>(async () => await project.CompileToRealAssembly());
        }

        [Fact]
        public async Task GetEnumValueInfoWorks()
        {
            var expected = new EnumValueInfo("User", 0, new Attribute[0]);

            var assembly = await TestProject.Project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Apparatus_AOT_Reflection_Playground_UserKindExtensions");
            Assert.NotNull(extension);

            var method = extension.GetMethod("GetEnumValueInfo", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var entry = (IEnumValueInfo)method.Invoke(null, new object[] { UserKind.User });
            Assert.Equal(expected, entry);
        }

        [Fact]
        public async Task GetEnumValueInfoWorksWithAttributes()
        {
            var expected = new EnumValueInfo("Admin", 1, new Attribute[] { new DescriptionAttribute("Admin user") });

            var assembly = await TestProject.Project.CompileToRealAssembly();

            var extension = assembly.GetType("Apparatus.AOT.Reflection.Apparatus_AOT_Reflection_Playground_UserKindExtensions");
            Assert.NotNull(extension);

            var method = extension.GetMethod("GetEnumValueInfo", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var entry = (IEnumValueInfo)method.Invoke(null, new object[] { UserKind.Admin });
            Assert.Equal(expected, entry);
        }
    }
}