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

            var extensionType =
                context.Compilation.GetTypeByMetadataName("AttributesExtractor.AttributesExtractorExtensions");
            var extensionMethod = extensionType.GetMembers().OfType<IMethodSymbol>().First(o => o.Name == "GetProperties");
            var genericHelperType =
                context.Compilation.GetTypeByMetadataName("AttributesExtractor.GenericHelper");
            var bootstrapMethod = genericHelperType.GetMembers().OfType<IMethodSymbol>().First(o => o.Name == "Bootstrap");

            var processed = new HashSet<string>();
            foreach (var memberAccess in receiver.MemberAccess)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                var semanticModel = context.Compilation.GetSemanticModel(memberAccess.SyntaxTree);
                var symbol = ModelExtensions.GetSymbolInfo(semanticModel, memberAccess.Name);
                if (!(symbol.Symbol is IMethodSymbol methodSymbol))
                {
                    continue;
                }

                if (!SymbolEqualityComparer.Default.Equals(extensionMethod, methodSymbol.ReducedFrom) &&
                    !SymbolEqualityComparer.Default.Equals(bootstrapMethod, methodSymbol.ConstructedFrom) )
                {
                    continue;
                }

                var typeToBake = methodSymbol.TypeArguments.First();
                if (typeToBake is ITypeParameterSymbol)
                {
                    continue;
                }
                
                if (processed.Contains(typeToBake.ToGlobalName()))
                {
                    continue;
                }

                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                var propertyAndAttributes = typeToBake
                    .GetAllMembers()
                    .OfType<IPropertySymbol>()
                    .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public))
                    .GroupBy(o => o.Name)
                    .Select(o =>
                    {
                        if (o.Count() > 1)
                        {
                            return o.Last();
                        }

                        return o.First();
                    })
                    .ToArray();

                var source = $@"
using System;
using System.Linq;

namespace AttributesExtractor
{{
    public static class {typeToBake.ToFileName()}
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            MetadataStore<{typeToBake.ToGlobalName()}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<string, IPropertyInfo>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<string, IPropertyInfo>>(new global::System.Collections.Generic.Dictionary<string, IPropertyInfo>
        {{
{propertyAndAttributes.Select(o => 
$@"            {{ ""{o.Name}"", new global::AttributesExtractor.PropertyInfo<{typeToBake.ToGlobalName()},{o.Type.ToGlobalName()}>(
                        ""{o.Name}"", 
                        new global::AttributesExtractor.AttributeData[] 
                        {{
                            {GenerateAttributes(o.GetAttributes())}
                        }}, 
                        {GenerateGetterAndSetter(o)})
                }},")
                        .JoinWithNewLine()}
        }}); 


        public static global::System.Collections.Generic.IReadOnlyDictionary<string, IPropertyInfo> GetProperties(this {typeToBake.ToGlobalName()} value)
        {{
            return _lazy.Value;
        }}   
    }}
}}
";
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
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
                {
                    var parameters = o.AttributeConstructor.Parameters
                        .Select((parameter, i) => new KeyValuePair<string, TypedConstant>(parameter.Name, o.ConstructorArguments[i]))
                        .Select(Convert);
                    
                    return $@"new global::AttributesExtractor.AttributeData(typeof({o.AttributeClass.ToGlobalName()}), new global::System.Collections.Generic.Dictionary<string, object>{{ {parameters.Join()} }}),";
                })
                .JoinWithNewLine();

        private static string Convert(KeyValuePair<string, TypedConstant> pair)
        {
            if (pair.Value.Kind == TypedConstantKind.Array && !pair.Value.IsNull)
            {
                return $@"{{""{pair.Key}"", new[] {pair.Value.ToCSharpString()}}}";
            }

            return $@"{{""{pair.Key}"", {pair.Value.ToCSharpString()}}}";
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
                (memberAccess.Name.ToString() == "GetProperties" || 
                 memberAccess.Name is GenericNameSyntax genericNameSyntax && genericNameSyntax.Identifier.ToString() == "Bootstrap"))
            {
                MemberAccess.Add(memberAccess);
            }
        }
    }
}