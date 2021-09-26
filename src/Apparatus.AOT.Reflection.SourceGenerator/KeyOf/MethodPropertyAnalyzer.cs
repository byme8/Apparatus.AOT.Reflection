using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is InvocationExpressionSyntax invocation))
            {
                return;
            }

            var possibleMethodSymbol = context.SemanticModel.GetSpeculativeSymbolInfo(invocation.SpanStart, invocation, SpeculativeBindingOption.BindAsExpression);
            if (!(possibleMethodSymbol.Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            KeyOfAnalyzer.AnalyzeKeyOfUsages(
                context, 
                methodSymbol.Parameters, 
                invocation.ArgumentList.Arguments);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName);
    }
}