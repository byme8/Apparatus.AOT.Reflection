using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AttributesExtractor.Playground;
using AttributesExtractor.SourceGenerator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace AttributesExtractor.Tests.Utils
{
    public static class TestExtensions
    {
        public static async Task<Project> ReplacePartOfDocumentAsync(this Project project, string documentName,
            string textToReplace, string newText)
        {
            var document = project.Documents.First(o => o.Name == documentName);
            var text = await document.GetTextAsync();
            return document
                .WithText(SourceText.From(text.ToString().Replace(textToReplace, newText)))
                .Project;
        }
        
        public static async Task<IPropertyInfo[]> ExecuteTest(this Project project, User user = null)
        {
            var assembly = await project.CompileToRealAssembly();

            var extension = assembly.GetType("AttributesExtractor.AttributesExtractor_Playground_UserExtensions");
            Assert.NotNull(extension);

            var method = extension.GetMethod("GetAttributes", BindingFlags.Static | BindingFlags.Public);
            Assert.NotNull(method);

            var entries = (IPropertyInfo[])method.Invoke(null, new[] { user ?? new User() });
            
            return entries;
        }
        
        public static string Stringify(IPropertyInfo propertyInfo)
        {
            return
                $"{propertyInfo.Name}{propertyInfo.Attributes.Select(o => $"{o.Type.FullName}{o.Parameters.Select(oo => oo?.ToString()).Join()}").Join()}";
        }

        public static async Task<Project> ReplacePartOfDocumentAsync(this Project project, string documentName,
            params (string TextToReplace, string NewText)[] places)
        {
            foreach (var place in places)
            {
                project = await project.ReplacePartOfDocumentAsync(documentName, place.TextToReplace, place.NewText);
            }

            return project;
        }
        
        public static async Task<Project> ReplacePartOfDocumentAsync(this Project project,
            params (string ProjectName, string DocumentName, string TextToReplace, string NewText)[] places)
        {
            var solution = project.Solution;
            foreach (var place in places)
            {
                var newProject = await solution.Projects
                    .First(o => o.Name == place.ProjectName)
                    .ReplacePartOfDocumentAsync(place.DocumentName, (place.TextToReplace, place.NewText));

                solution = newProject.Solution;
            }
            
            return solution.Projects.First(o => o.Name == project.Name);
        }

        public static async Task<Assembly> CompileToRealAssembly(this Project project)
        {
            var compilation = await project.GetCompilationAsync();
            var error = compilation.GetDiagnostics().FirstOrDefault(o => o.Severity == DiagnosticSeverity.Error);
            if (error != null)
            {
                throw new Exception(error.GetMessage());
            }

            using (var memoryStream = new MemoryStream())
            {
                compilation.Emit(memoryStream);
                var bytes = memoryStream.ToArray();
                var assembly = Assembly.Load(bytes);

                return assembly;
            }
        }
    }
}