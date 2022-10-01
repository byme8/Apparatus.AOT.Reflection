using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Apparatus.AOT.Reflection.SourceGenerator.KeyOf;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    [Generator]
    public class AotPropertiesReflectionSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var types = context.SyntaxProvider.CreateSyntaxProvider(
                    FindClasses,
                    (syntaxContext, token) => syntaxContext.Node switch
                    {
                        GenericNameSyntax genericNameSyntax => GetTypeFromGenericNameSyntax(syntaxContext, genericNameSyntax, token),
                        InvocationExpressionSyntax invocationExpressionSyntax => GetTypeFromInvocation(syntaxContext, invocationExpressionSyntax, token),
                        AttributeSyntax attributeSyntax => GetTypeFromAttribute(syntaxContext, attributeSyntax, token),
                        _ => null
                    })
                .SelectMany((o, c) => o)
                .Where(o => o != null)
                .WithComparer(SymbolEqualityComparer.Default);

            context.RegisterSourceOutput(types.Collect(), Generate!);
        }

        private IEnumerable<ITypeSymbol?> GetTypeFromAttribute(GeneratorSyntaxContext syntaxContext, AttributeSyntax attributeSyntax, CancellationToken token)
        {
            if (attributeSyntax.Parent is not AttributeListSyntax { Parent: BaseTypeDeclarationSyntax baseTypeDeclarationSyntax })
            {
                yield break;
            }
            
            var possibleType = syntaxContext.SemanticModel.GetDeclaredSymbol(baseTypeDeclarationSyntax, token);
            if (possibleType is ITypeSymbol type)
            {
                yield return type;
            }
        }

        private IEnumerable<ITypeSymbol?> GetTypeFromInvocation(GeneratorSyntaxContext syntaxContext, InvocationExpressionSyntax invocationExpressionSyntax, CancellationToken token)
        {
            var possibleMethod = syntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax, token);
            if (possibleMethod.Symbol is not IMethodSymbol methodSymbol)
            {
                yield break;
            }

            yield return HandleGetProperties(syntaxContext, methodSymbol);

            var keyOfs = HandleKeyofs(syntaxContext, methodSymbol);
            foreach (var keyOf in keyOfs)
            {
                yield return keyOf;
            }
        }

        private IEnumerable<ITypeSymbol?> HandleKeyofs(GeneratorSyntaxContext syntaxContext, IMethodSymbol methodSymbol)
        {
            var keyOf = syntaxContext.SemanticModel.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.KeyOf`1");

            if (methodSymbol.ReturnType is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.ConstructedFrom.Equals(keyOf, SymbolEqualityComparer.Default))
            {
                var typeArgument = namedTypeSymbol.TypeArguments.First();
                yield return typeArgument;
            }
            
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (parameter.Type is INamedTypeSymbol parameterType &&
                    parameterType.ConstructedFrom.Equals(keyOf, SymbolEqualityComparer.Default))
                {
                    var typeArgument = parameterType.TypeArguments.First();
                    yield return typeArgument;
                }
            }
        }

        private static ITypeSymbol? HandleGetProperties(GeneratorSyntaxContext syntaxContext, IMethodSymbol methodSymbol)
        {
            var extensionType = syntaxContext.SemanticModel.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.AOTReflectionExtensions");
            var extensionMethod = extensionType?
                .GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(o => o.Name == "GetProperties");

            if (!SymbolEqualityComparer.Default.Equals(extensionMethod, methodSymbol.ReducedFrom))
            {
                return null;
            }

            return methodSymbol.TypeArguments.First();
        }

        private IEnumerable<ITypeSymbol?> GetTypeFromGenericNameSyntax(GeneratorSyntaxContext syntaxContext, GenericNameSyntax genericNameSyntax, CancellationToken token)
        {
            var possibleType = syntaxContext.SemanticModel.GetSymbolInfo(genericNameSyntax, token);
            if (possibleType.Symbol is not INamedTypeSymbol type)
            {
                yield break;
            }
            
            var keyOf = syntaxContext.SemanticModel.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.KeyOf`1");

            if (!SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, keyOf))
            {
                yield break;
            }
                
            yield return type.TypeArguments.First();
        }

        private void Generate(SourceProductionContext context, ImmutableArray<ITypeSymbol> types)
        {
            var uniqueTypes = types.ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var type in uniqueTypes)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (type is ITypeParameterSymbol)
                {
                    continue;
                }
                
                if (type is IErrorTypeSymbol)
                {
                    continue;
                }

                if (type.TypeKind == TypeKind.Enum)
                {
                    continue;
                }

                var source = GenerateExtensionForProperties(type);
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                context.AddSource(type.ToFileName(), source);
            }
        }

        private bool FindClasses(SyntaxNode syntaxNode, CancellationToken cancellationToken)
            => syntaxNode switch
            {
                InvocationExpressionSyntax => true,
                GenericNameSyntax { Identifier.Text: "KeyOf" } => true,
                AttributeSyntax attributeSyntax => attributeSyntax.Name.ToString().Contains("AOTReflection"),
                _ => false
            };

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
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{{
    {KeyOfAnalyzer.CodeGenerationAttribute}
    public static class {typeToBake.ToSafeGlobalName()}Extensions
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            var type = typeof({typeGlobalName});
            TypedMetadataStore.Types.Add(type, typeInfo);
        }}

        private static global::System.Collections.Generic.IReadOnlyDictionary<IKeyOf, IPropertyInfo> typeInfo = new global::System.Collections.Generic.Dictionary<IKeyOf, IPropertyInfo>
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
        }}; 


        {GetVisibility(typeToBake)} static global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeGlobalName}>, IPropertyInfo> GetProperties(this {typeGlobalName} value)
        {{
            return MetadataStore<{typeGlobalName}>.Data;
        }}   
    }}
}}
";
            return source;
        }

        private string GetVisibility(ITypeSymbol typeToBake)
            => typeToBake.DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                _ => "private"
            };

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