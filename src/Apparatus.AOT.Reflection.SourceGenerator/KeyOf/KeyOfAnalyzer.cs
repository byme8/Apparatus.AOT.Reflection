using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    public class KeyOfAnalyzer
    {
        public static void AnalyzeKeyOfUsages(
            SyntaxNodeAnalysisContext context,
            IEnumerable<IParameterSymbol> parameters,
            IEnumerable<ArgumentSyntax> arguments)
        {
            var keyOfs = parameters
                .Select((o, i) => (Index: i, Type: (INamedTypeSymbol)o.Type))
                .Where(o => o.Type.ConstructedFrom.ToString().StartsWith("Apparatus.AOT.Reflection.KeyOf<"))
                .ToArray();

            if (!keyOfs.Any())
            {
                return;
            }

            var valuesAsText = arguments
                .Select((o, i) =>
                {
                    if (o.Expression is LiteralExpressionSyntax literal)
                    {
                        return (Index: i, o.Expression, Value: literal.Token.Text.Trim('"'));
                    }

                    if (o.Expression is InvocationExpressionSyntax expressionInvocation &&
                        expressionInvocation.Expression is IdentifierNameSyntax identifier &&
                        identifier.Identifier.Text == "nameof" &&
                        expressionInvocation.ArgumentList.Arguments.First().Expression is ExpressionSyntax expression)
                    {
                        if (expression is MemberAccessExpressionSyntax memberAccess)
                        {
                            return (Index: i, memberAccess.Name, Value: memberAccess.Name.Identifier.Text);
                        }
                        
                        if (expression is IdentifierNameSyntax nameSyntax)
                        {
                            return (Index: i, nameSyntax, Value: nameSyntax.Identifier.Text);
                        }
                    }

                    var possibleConstant = context.SemanticModel
                        .GetSpeculativeSymbolInfo(
                            o.Expression.SpanStart,
                            o.Expression,
                            SpeculativeBindingOption.BindAsExpression);

                    if (possibleConstant.Symbol is ILocalSymbol localSymbol && localSymbol.HasConstantValue)
                    {
                        return (Index: i, o.Expression, Value: localSymbol.ConstantValue.ToString());
                    }

                    return (Index: i, null, null);
                })
                .Where(o => o.Expression != null)
                .ToArray();

            foreach (var keyOf in keyOfs)
            {
                var (index, type) = keyOf;
                var propertyNameLiteral = valuesAsText.FirstOrDefault(o => o.Index == index);
                if (propertyNameLiteral == default)
                {
                    continue;
                }

                var propertyName = propertyNameLiteral.Value;
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