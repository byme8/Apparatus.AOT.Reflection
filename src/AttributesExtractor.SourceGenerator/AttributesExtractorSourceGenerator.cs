using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
            var initialMethod = initialType.GetMembers().OfType<IMethodSymbol>().First(o => o.Name == "GetProperties");
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
                    .ToArray();

                var source = $@"
using System;

namespace AttributesExtractor
{{
    public static class {typeToBake.ToFileName()}
    {{
        private static global::System.Lazy<global::AttributesExtractor.IPropertyInfo[]> _lazy = new global::System.Lazy<global::AttributesExtractor.IPropertyInfo[]>(new[]
        {{
{propertyAndAttributes.Select(o => 
$@"            new global::AttributesExtractor.PropertyInfo<{typeToBake.ToGlobalName()},{o.Type.ToGlobalName()}>(
                        ""{o.Name}"", 
                        new[] 
                        {{
                            {GenerateAttributes(o.GetAttributes())}
                        }}, 
                        {GenerateGetterAndSetter(o)}),")
                        .JoinWithNewLine()}
        }}); 


        public static IPropertyInfo[] GetProperties(this {typeToBake.ToGlobalName()} value)
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

        private string GenerateGetterAndSetter(IPropertySymbol propertySymbol)
        {
            var sb = new StringBuilder();
            if (propertySymbol.GetMethod != null && propertySymbol.GetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                sb.Append($"instance => instance.{propertySymbol.Name}");
            }
            else
            {
                sb.Append("null");
            }

            sb.Append(", ");
            
            if (propertySymbol.SetMethod != null&& propertySymbol.SetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                sb.Append($"(instance, value) => instance.{propertySymbol.Name} = value");
            }
            else
            {
                sb.Append("null");
            }

            return sb.ToString();
        }

        private static string GenerateAttributes(ImmutableArray<AttributeData> attributes)
            => attributes
                .Select(o =>
                    $@"new AttributeData(typeof({o.AttributeClass.ToGlobalName()}){(o.ConstructorArguments.Any() ? "," : "")} {o.ConstructorArguments.Select(Convert).Join()}),")
                .JoinWithNewLine();

        private static string Convert(TypedConstant typedConstant)
        {
            if (typedConstant.Kind == TypedConstantKind.Array && !typedConstant.IsNull)
            {
                return $"new[] {typedConstant.ToCSharpString()}";
            }

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
                memberAccess.Name.ToString() == "GetProperties")
            {
                MemberAccess.Add(memberAccess);
            }
        }
    }
}