using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IndexPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ElementAccessExpression);
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is ElementAccessExpressionSyntax indexExpression))
            {
                return;
            }

            var possibleMethodSymbol = context.SemanticModel.GetSpeculativeSymbolInfo(indexExpression.SpanStart, indexExpression, SpeculativeBindingOption.BindAsExpression);
            if (!(possibleMethodSymbol.Symbol is IPropertySymbol propertySymbol))
            {
                return;
            }

            KeyOfAnalyzer.AnalyzeKeyOfUsages(
                context,
                propertySymbol.Parameters,
                indexExpression.ArgumentList.Arguments);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName, 
                DiagnosticDescriptors.ImpossibleToGetThePropertyName);
    }
}