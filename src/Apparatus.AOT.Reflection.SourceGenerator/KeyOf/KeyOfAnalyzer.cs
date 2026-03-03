using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    public class KeyOfAnalyzer
    {
        public static void AnalyzeKeyOfUsages(
            OperationAnalysisContext context,
            ImmutableArray<IParameterSymbol> parameters,
            ImmutableArray<IArgumentOperation> arguments)
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
                    var value = UnwrapConversion(o.Value);

                    if (value.ConstantValue.HasValue)
                    {
                        return (Index: i, Syntax: o.Syntax, Value: value.ConstantValue.Value?.ToString());
                    }

                    if (value is INameOfOperation nameOf)
                    {
                        return (Index: i, Syntax: o.Syntax, Value: nameOf.ConstantValue.Value?.ToString());
                    }

                    return (Index: i, Syntax: o.Syntax, Value: (string)null);
                })
                .ToArray();

            foreach (var keyOf in keyOfs)
            {
                var (index, type) = keyOf;
                var propertyNameLiteral = valuesAsText.FirstOrDefault(o => o.Index == index);

                if (propertyNameLiteral.Value == default)
                {
                    var argValue = UnwrapConversion(arguments[index].Value);
                    var argType = argValue.Type as INamedTypeSymbol;

                    if (IsKeyOf(argType))
                    {
                        continue;
                    }

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            DiagnosticDescriptors.ImpossibleToGetThePropertyName,
                            propertyNameLiteral.Syntax.GetLocation()));
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
                            propertyNameLiteral.Syntax.GetLocation(),
                            propertyName,
                            type.TypeArguments.First().ToDisplayString()));
                }
            }
        }

        private static IOperation UnwrapConversion(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }

            return operation;
        }

        public static bool IsKeyOf(INamedTypeSymbol? type)
            => type?.ConstructedFrom.ToString().StartsWith("Apparatus.AOT.Reflection.KeyOf<") ?? false;

        public static string Version { get; } = typeof(KeyOfAnalyzer).Assembly.GetName().Version.ToString();

        public static string CodeGenerationAttribute { get; } = $@"[System.CodeDom.Compiler.GeneratedCode(""AOT.Reflection"", ""{Version}"")]";
    }
}