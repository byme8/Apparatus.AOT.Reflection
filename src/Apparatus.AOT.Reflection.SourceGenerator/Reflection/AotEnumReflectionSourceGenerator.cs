﻿using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    [Generator]
    public class AotEnumReflectionSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var types = context.SyntaxProvider.CreateSyntaxProvider(
                    FindAOTReflectionAttributeOnType,
                    (syntaxContext, token) => syntaxContext.Node switch
                    {
                        BaseTypeDeclarationSyntax baseTypeDeclarationSyntax => GetTypeFromBaseDeclaration(syntaxContext, baseTypeDeclarationSyntax, token),
                        InvocationExpressionSyntax invocationExpressionSyntax => GetTypeFromInvocation(syntaxContext, invocationExpressionSyntax, token),
                        _ => null
                    })
                .Where(o => o is not null)
                .WithComparer(SymbolEqualityComparer.Default);


            context.RegisterImplementationSourceOutput(types.Collect(), Generate!);
        }

        private void Generate(SourceProductionContext context, ImmutableArray<ITypeSymbol> types)
        {
            var uniqueTypes = types.ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

            foreach (var type in uniqueTypes)
            {
                if (type.TypeKind != TypeKind.Enum)
                {
                    continue;
                }

                var source = GenerateExtensionForEnum(type);
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                context.AddSource(type.ToFileName(), source);
            }
        }

        private static ITypeSymbol? GetTypeFromInvocation(GeneratorSyntaxContext syntaxContext, InvocationExpressionSyntax invocation, CancellationToken token)
        {
            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            var possibleMethod = syntaxContext.SemanticModel.GetSymbolInfo(memberAccess.Name, token);
            if (possibleMethod.Symbol is not IMethodSymbol methodSymbol)
            {
                return null;
            }

            return methodSymbol.TypeArguments.First();
        }

        private static ITypeSymbol? GetTypeFromBaseDeclaration(GeneratorSyntaxContext syntaxContext, BaseTypeDeclarationSyntax baseDeclaration, CancellationToken token)
        {
            var possibleType = syntaxContext.SemanticModel.GetDeclaredSymbol(baseDeclaration, token);
            if (possibleType is not ITypeSymbol { TypeKind: TypeKind.Enum } typeSymbol)
            {
                return null;
            }

            return typeSymbol;
        }

        private bool FindAOTReflectionAttributeOnType(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is BaseTypeDeclarationSyntax typeDeclarationSyntax &&
                typeDeclarationSyntax.AttributeLists
                    .Any(o => o.Attributes
                        .Any(oo => oo.Name.ToString().Contains("AOTReflection"))))
            {
                return true;
            }

            if (syntaxNode is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } &&
                memberAccess.Name.ToString() is "GetEnumValueInfo" or "ToInt")
            {
                return true;
            }

            if (syntaxNode is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Expression: IdentifierNameSyntax { Identifier.Text: "EnumHelper" },
                        Name: GenericNameSyntax { Identifier.Text: "GetEnumInfo" or "FromInt" }
                    }
                })
            {
                return true;
            }

            return false;
        }

        private string GenerateExtensionForEnum(ITypeSymbol typeToBake)
        {
            var typeGlobalName = typeToBake.ToGlobalName();
            var fields = typeToBake
                .GetMembers()
                .OfType<IFieldSymbol>()
                .ToArray();
            
            var source = $@"
using System;
using System.Linq;
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{{
    internal static class {typeToBake.ToSafeGlobalName()}Extensions
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            EnumMetadataStore<{typeGlobalName}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>>(new global::System.Collections.Generic.Dictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>
        {{
{fields.Select(o => $@"
            {{ {typeGlobalName}.{o.Name}, new EnumValueInfo<{typeGlobalName}>(
                ""{o.Name}"", 
                {(GetDescription(o) is string description ? $@"""{description}""" : "null")},
                {o.ConstantValue},
                {typeGlobalName}.{o.Name}, 
                new Attribute[] 
                {{ 
                    {o.GetAttributes().GenerateAttributes()}
                }})
            }},")
    .JoinWithNewLine()
}
        }}); 
    }}
}}
";
            return source;
        }

        private static string? GetDescription(IFieldSymbol field)
        {
            var description = field.GetAttributes()
                .FirstOrDefault(o => o.AttributeClass?.Name == "DescriptionAttribute")?
                .ConstructorArguments
                .FirstOrDefault()
                .Value?
                .ToString();
            
            return description;
        }
    }
}