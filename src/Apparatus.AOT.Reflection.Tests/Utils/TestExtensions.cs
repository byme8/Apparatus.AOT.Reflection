﻿using System.Collections.Immutable;
using System.Reflection;
using Apparatus.AOT.Reflection.SourceGenerator.KeyOf;
using Apparatus.AOT.Reflection.SourceGenerator.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Apparatus.AOT.Reflection.Tests.Utils;

public static class TestExtensions
{
    public static async Task<Project> ReplacePartOfDocumentAsync(this Project project,
        string documentName,
        string textToReplace,
        string newText)
    {
        var document = project.Documents.First(o => o.Name == documentName);
        var text = await document.GetTextAsync();
        return document
            .WithText(SourceText.From(text.ToString().Replace(textToReplace, newText)))
            .Project;
    }

    public static async Task<object> ExecuteTest(this Project project, string source)
    {
        project = await project.ReplacePartOfDocumentAsync(
            "Program.cs",
            ("return null; // place to place execute", source));

        var assembly = await project.CompileToRealAssembly();

        var extension = assembly.GetType("Apparatus.AOT.Reflection.Playground.Program");
        Assert.NotNull(extension);

        var method = extension.GetMethod("Execute", BindingFlags.Static | BindingFlags.Public);
        Assert.NotNull(method);

        return method.Invoke(null, null);
    }

    public static async Task<Project> ReplacePartOfDocumentAsync(this Project project,
        string documentName,
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
        var analyzerResults = await compilation
            .WithAnalyzers(ImmutableArray.Create(new DiagnosticAnalyzer[]
            {
                new AOTReflectionAnalyzer(),
                new IndexPropertyAnalyzer(),
                new MethodPropertyAnalyzer(),
                new DontUseKeyOfConstructorAnalyzer(),
            }))
            .GetAllDiagnosticsAsync();

        var error = compilation.GetDiagnostics().Concat(analyzerResults)
            .FirstOrDefault(o => o.Severity == DiagnosticSeverity.Error);

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