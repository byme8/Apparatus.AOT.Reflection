using System;
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
                .Where(o => o.RefKind != RefKind.Out)
                .Select((o, i) => (Index: i, Type: o.Type as INamedTypeSymbol))
                .Where(o => IsKeyOf(o.Type))
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

                    if (possibleConstant.Symbol is IFieldSymbol fieldSymbol && fieldSymbol.HasConstantValue)
                    {
                        return (Index: i, o.Expression, Value: fieldSymbol.ConstantValue.ToString());
                    }

                    return (Index: i, o.Expression, null);
                })
                .ToArray();

            foreach (var keyOf in keyOfs)
            {
                var (index, type) = keyOf;
                var propertyNameLiteral = valuesAsText.FirstOrDefault(o => o.Index == index);

                if (propertyNameLiteral.Value == default)
                {
                    var propertySymbol = context.SemanticModel
                        .GetSpeculativeSymbolInfo(
                            propertyNameLiteral.Expression.SpanStart,
                            propertyNameLiteral.Expression,
                            SpeculativeBindingOption.BindAsExpression);

                    if (propertySymbol.Symbol is ILocalSymbol localSymbol &&
                        IsKeyOf(localSymbol.Type as INamedTypeSymbol))
                    {
                        continue;
                    }

                    if (propertySymbol.Symbol is IParameterSymbol parameterSymbol &&
                        IsKeyOf(parameterSymbol.Type as INamedTypeSymbol))
                    {
                        continue;
                    }

                    if (propertySymbol.Symbol is IFieldSymbol fieldSymbol &&
                        IsKeyOf(fieldSymbol.Type as INamedTypeSymbol))
                    {
                        continue;
                    }

                    if (propertySymbol.Symbol is IPropertySymbol property &&
                        IsKeyOf(property.Type as INamedTypeSymbol))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ImpossibleToGetThePropertyName,
                            propertyNameLiteral.Expression.GetLocation()));
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

        public static bool IsKeyOf(INamedTypeSymbol? type)
            => type?.ConstructedFrom.ToString().StartsWith("Apparatus.AOT.Reflection.KeyOf<") ?? false;

        public static string Version { get; } = typeof(KeyOfAnalyzer).Assembly.GetName().Version.ToString();

        public static string CodeGenerationAttribute { get; } = $@"[System.CodeDom.Compiler.GeneratedCode(""AOT.Reflection"", ""{Version}"")]";
    }
}