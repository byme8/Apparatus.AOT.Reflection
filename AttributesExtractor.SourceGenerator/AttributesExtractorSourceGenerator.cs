using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AttributesExtractor.SourceGenerator
{
    [Generator]
    public class AttributesExtractorSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AttributesExtractorSyntaxNotification receiver))
            {
                return;
            }

            var initialType =
                context.Compilation.GetTypeByMetadataName("AttributesExtractor.AttributesExtractorExtensions");
            var initialMethod = initialType.GetMembers().OfType<IMethodSymbol>().First(o => o.Name == "GetAttributes");
            var processed = new HashSet<string>();
            foreach (var memberAccess in receiver.MemberAccess)
            {
                var semanticModel = context.Compilation.GetSemanticModel(memberAccess.SyntaxTree);
                var symbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess.Name);
                if (!(symbol.Symbol is IMethodSymbol methodSymbol))
                {
                    continue;
                }

                if (!SymbolEqualityComparer.Default.Equals(initialMethod, methodSymbol.ReducedFrom))
                {
                    continue;
                }

                var typeToBake = methodSymbol.TypeArguments.First();
                if (processed.Contains(typeToBake.ToGlobalName()))
                {
                    continue;
                }
                
                var propertyAndAttributes = typeToBake
                    .GetAllMembers()
                    .OfType<IPropertySymbol>()
                    .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public))
                    .Select(o => new { o.Name, Attributes = o.GetAttributes() })
                    .Where(o => o.Attributes.Any())
                    .ToArray();

                var source = $@"
using System;

namespace AttributesExtractor
{{
    public static class {typeToBake.ToFileName()}
    {{
        private static global::System.Lazy<global::AttributesExtractor.Entry[]> _lazy = new global::System.Lazy<global::AttributesExtractor.Entry[]>(new[]
        {{
{propertyAndAttributes.Select(o => $@"new global::AttributesExtractor.Entry(""{o.Name}"", new[] {{{GenerateAttributes(o.Attributes)}}}),").JoinWithNewLine()}
        }}); 


        public static Entry[] GetAttributes(this {typeToBake.ToGlobalName()} value)
        {{
            return _lazy.Value;
        }}   
    }}
}}
";
                context.AddSource(typeToBake.ToFileName(), source);
                processed.Add(typeToBake.ToGlobalName());
            }
        }

        private static string GenerateAttributes(ImmutableArray<AttributeData> attributes)
            => attributes
                .Select(o =>
                    $@"new Entry.EntryAttribute(typeof({o.AttributeClass.ToGlobalName()}){(o.ConstructorArguments.Any() ? "," : "")} {o.ConstructorArguments.Select(Convert).Join()})")
                .Join();

        private static string Convert(TypedConstant typedConstant)
        {
            return typedConstant.ToCSharpString();
        }
        
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AttributesExtractorSyntaxNotification());
        }
    }

    public class AttributesExtractorSyntaxNotification : ISyntaxReceiver
    {
        public List<MemberAccessExpressionSyntax> MemberAccess { get; } = new List<MemberAccessExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.ToString() == "GetAttributes")
            {
                MemberAccess.Add(memberAccess);
            }
        }
    }
}