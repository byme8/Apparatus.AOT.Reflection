using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DontUseKeyOfConstructorAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.ObjectCreationExpression);
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is ObjectCreationExpressionSyntax creation))
            {
                return;
            }

            var symbol = context.SemanticModel
                .GetSpeculativeSymbolInfo(
                    creation.Type.SpanStart,
                    creation.Type,
                    SpeculativeBindingOption.BindAsTypeOrNamespace);

            if (symbol.Symbol is INamedTypeSymbol namedTypeSymbol && 
                KeyOfAnalyzer.IsKeyOf(namedTypeSymbol))
            {
                var codeGenerationAttribute = context.Compilation
                    .GetTypeByMetadataName("System.CodeDom.Compiler.GeneratedCodeAttribute");
                
                if (context.ContainingSymbol?.ContainingSymbol.GetAttributes()
                    .Any(o => SymbolEqualityComparer.Default.Equals(o.AttributeClass, codeGenerationAttribute)) ?? false)
                {
                    return;
                }
                
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.DontUseKeyOfConstructor,
                        creation.GetLocation()));
            }

        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.DontUseKeyOfConstructor);
    }
}