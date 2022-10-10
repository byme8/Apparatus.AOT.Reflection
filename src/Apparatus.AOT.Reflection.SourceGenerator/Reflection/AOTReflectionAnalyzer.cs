using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AOTReflectionAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.EnumDoesntHaveProperties);
        
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.InvocationExpression);
        }

        private void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.ToString() == "GetProperties")
            {
                var extensionType =
                    context.Compilation.GetTypeByMetadataName("AOTReflectionExtensions");
                var extensionMethod =
                    extensionType!.GetMembers().OfType<IMethodSymbol>().First(o => o.Name == "GetProperties");
                
                var semanticModel = context.SemanticModel;
                var symbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess.Name);
                if (!(symbol.Symbol is IMethodSymbol methodSymbol))
                {
                    return;
                }

                if (!SymbolEqualityComparer.Default.Equals(extensionMethod, methodSymbol.ReducedFrom))
                {
                    return;
                }

                var typeToBake = methodSymbol.TypeArguments.First();
                if (typeToBake is ITypeParameterSymbol)
                {
                    return;
                }

                if (typeToBake.TypeKind == TypeKind.Enum)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.EnumDoesntHaveProperties,
                        memberAccess.GetLocation(), 
                        typeToBake.Name));
                    
                    return;
                }
            }
        }
    }
}