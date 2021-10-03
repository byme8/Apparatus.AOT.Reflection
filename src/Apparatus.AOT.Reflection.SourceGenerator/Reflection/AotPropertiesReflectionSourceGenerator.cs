using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apparatus.AOT.Reflection.SourceGenerator.KeyOf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    [Generator]
    public class AotPropertiesReflectionSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AotPropertiesReflectionSyntaxNotification receiver))
            {
                return;
            }

            var extensionType = context.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.AOTReflectionExtensions");
            var extensionMethod = extensionType?
                .GetMembers()
                .OfType<IMethodSymbol>()
                .First(o => o.Name == "GetProperties");

            var typesToBake = AnalyzeInvocations().Concat(AnalyzeTypes())
                .Where(o => o != null)
                .Distinct(SymbolEqualityComparer.Default);

            var processed = new HashSet<string>();
            foreach (var typeToBake in typesToBake.OfType<ITypeSymbol>())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (processed.Contains(typeToBake.ToGlobalName()))
                {
                    continue;
                }

                if (typeToBake is ITypeParameterSymbol)
                {
                    continue;
                }

                if (typeToBake.TypeKind == TypeKind.Enum)
                {
                    continue;
                }

                var source = GenerateExtensionForProperties(typeToBake);
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                context.AddSource(typeToBake.ToFileName(), source);
                processed.Add(typeToBake.ToGlobalName());
            }

            IEnumerable<ISymbol> AnalyzeInvocations()
                => receiver.Invocations.SelectMany(o =>
                {
                    var methodSymbol = GetMethodSymbol(context, o);
                    if (methodSymbol is null)
                    {
                        return Enumerable.Empty<ITypeSymbol>();
                    }

                    if (SymbolEqualityComparer.Default.Equals(extensionMethod, methodSymbol.ReducedFrom))
                    {
                        return new[] { methodSymbol.TypeArguments.First() };
                    }

                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        return Enumerable.Empty<ITypeSymbol>();
                    }

                    var keyofs = methodSymbol.Parameters
                        .Select(param => param.Type as INamedTypeSymbol)
                        .Where(param => param != null && KeyOfAnalyzer.IsKeyOf(param));

                    return keyofs.Select(keyof => keyof.TypeArguments.First());
                });

            IEnumerable<ISymbol> AnalyzeTypes()
                => receiver.GenericNames.Select(o =>
                {
                    var semanticModel = context.Compilation.GetSemanticModel(o.SyntaxTree);
                    var symbol = semanticModel
                        .GetSpeculativeSymbolInfo(o.SpanStart, o, SpeculativeBindingOption.BindAsTypeOrNamespace);

                    if (symbol.Symbol is INamedTypeSymbol typeSymbol && KeyOfAnalyzer.IsKeyOf(typeSymbol))
                    {
                        return typeSymbol.TypeArguments.First();
                    }

                    return null;
                });
        }

        private static IMethodSymbol GetMethodSymbol(GeneratorExecutionContext context, InvocationExpressionSyntax invocation)
        {
            var semanticModel = context.Compilation.GetSemanticModel(invocation.SyntaxTree);
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var memberSymbol = semanticModel.GetSymbolInfo(memberAccess.Name);
                if (memberSymbol.Symbol is IMethodSymbol memberMethodSymbol)
                {
                    return memberMethodSymbol;
                }
            }

            var symbol = semanticModel.GetSymbolInfo(invocation.Expression);
            if (symbol.Symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol;
            }

            return null;
        }

        private string GenerateExtensionForProperties(ITypeSymbol typeToBake)
        {
            var propertyAndAttributes = typeToBake
                .GetAllMembers()
                .GetPublicMembers<IPropertySymbol>()
                .GroupBy(o => o.Name)
                .Select(o =>
                {
                    if (o.Count() > 1)
                    {
                        // take newest property
                        return o.Last();
                    }

                    return o.First();
                })
                .ToArray();

            var typeGlobalName = typeToBake.ToGlobalName();
            var source = $@"
using System;
using System.Linq;

namespace Apparatus.AOT.Reflection
{{
    {KeyOfAnalyzer.CodeGenerationAttribute}
    public static class {typeToBake.ToFileName()}
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            MetadataStore<{typeGlobalName}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeGlobalName}>, IPropertyInfo>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeGlobalName}>, IPropertyInfo>>(new global::System.Collections.Generic.Dictionary<KeyOf<{typeGlobalName}>, IPropertyInfo>
        {{
{propertyAndAttributes.Select(o =>
        $@"            {{ new KeyOf<{typeGlobalName}>(""{o.Name}""), new global::Apparatus.AOT.Reflection.PropertyInfo<{typeGlobalName},{o.Type.ToGlobalName()}>(
                        ""{o.Name}"", 
                        new global::System.Attribute[] 
                        {{
                            {o.GetAttributes().GenerateAttributes()}
                        }}, 
                        {GenerateGetterAndSetter(o)})
                }},")
    .JoinWithNewLine()}
        }}); 


        {GetVisibility(typeToBake)} static global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeGlobalName}>, IPropertyInfo> GetProperties(this {typeGlobalName} value)
        {{
            return _lazy.Value;
        }}   
    }}
}}
";
            return source;
        }

        private string GetVisibility(ITypeSymbol typeToBake)
        {
            switch (typeToBake.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    return "public";
                case Accessibility.Internal:
                    return "internal";
                default:
                    return "private";
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AotPropertiesReflectionSyntaxNotification());
        }

        private string GenerateGetterAndSetter(IPropertySymbol propertySymbol)
        {
            var sb = new StringBuilder();
            if (propertySymbol.GetMethod != null &&
                propertySymbol.GetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                sb.Append($"instance => instance.{propertySymbol.Name}");
            }
            else
            {
                sb.Append("null");
            }

            sb.Append(", ");

            if (propertySymbol.SetMethod != null &&
                propertySymbol.SetMethod.DeclaredAccessibility.HasFlag(Accessibility.Public))
            {
                sb.Append($"(instance, value) => instance.{propertySymbol.Name} = value");
            }
            else
            {
                sb.Append("null");
            }

            return sb.ToString();
        }
    }
}