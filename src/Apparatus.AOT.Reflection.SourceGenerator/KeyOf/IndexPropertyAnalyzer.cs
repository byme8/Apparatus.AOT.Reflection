using System.Collections.Immutable;
using System.Linq;
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
            if (context.Node is ElementAccessExpressionSyntax indexExpression)
            {
                var possibleMethodSymbol = context.SemanticModel.GetSpeculativeSymbolInfo(indexExpression.SpanStart, indexExpression, SpeculativeBindingOption.BindAsExpression);
                if (possibleMethodSymbol.Symbol is IPropertySymbol propertySymbol)
                {
                    var keyOfs = propertySymbol.Parameters
                        .Select((o, i) => (Index: i, Type: (INamedTypeSymbol)o.Type))
                        .Where(o => o.Type.ConstructedFrom.ToString().StartsWith("Apparatus.AOT.Reflection.KeyOf<"))
                        .ToArray();

                    var valuesAsText = indexExpression.ArgumentList.Arguments
                        .Select((o, i) => (Index: i, Expression: o.Expression as LiteralExpressionSyntax))
                        .ToArray();

                    foreach (var keyOf in keyOfs)
                    {
                        var (index, type) = keyOf;
                        var propertyNameLiteral = valuesAsText.FirstOrDefault(o => o.Index == index);
                        if (propertyNameLiteral == default)
                        {
                            continue;
                        }

                        var propertyName = propertyNameLiteral.Expression.Token.Text.Trim('"');
                        var propertyAvailable = type.TypeArguments
                            .First()
                            .GetAllMembers()
                            .GetPublicMembers<IPropertySymbol>()
                            .Any(o => o.Name == propertyName);

                        if (!propertyAvailable)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName,
                                    propertyNameLiteral.Expression.GetLocation(),
                                    propertyName,
                                    type.TypeArguments.First().ToDisplayString()));
                        }
                    }
                }
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName);
    }
}