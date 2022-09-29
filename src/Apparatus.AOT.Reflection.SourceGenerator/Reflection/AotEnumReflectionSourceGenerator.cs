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
                FindGetEnumValueInfo,
                (syntaxContext, token) =>
                {
                    var invocation = (InvocationExpressionSyntax)syntaxContext.Node;
                    var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
                    var possibleMethod = syntaxContext.SemanticModel.GetSymbolInfo(memberAccess.Name, token);
                    if (possibleMethod.Symbol is not IMethodSymbol methodSymbol)
                    {
                        return null;
                    }

                    return methodSymbol.TypeArguments.First();
                })
                .WithComparer(SymbolEqualityComparer.Default);

            
            context.RegisterImplementationSourceOutput(types, Generate);
        }

        private void Generate(SourceProductionContext context, ITypeSymbol typeToBake)
        {
            if (typeToBake.TypeKind != TypeKind.Enum)
            {
                return;
            }

            var source = GenerateExtensionForEnum(typeToBake);
            if (context.CancellationToken.IsCancellationRequested)
            {
                return;
            }

            context.AddSource(typeToBake.ToFileName(), source);
        }

        private bool FindEnumReflectionUsage(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (memberAccess.Name.ToString() == "GetEnumValueInfo" ||
                 memberAccess.Name is GenericNameSyntax genericNameSyntax &&
                 genericNameSyntax.Identifier.ToString() == "GetEnumInfo"))
            {
                return true;
            }

            return false;
        }
        
        private bool FindGetEnumValueInfo(SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.ToString() == "GetEnumValueInfo")
            {
                return true;
            }

            return false;
        }

        private string GenerateExtensionForEnum(ITypeSymbol typeToBake)
        {
            var typeGlobalName = typeToBake.ToGlobalName();
            var source = $@"
using System;
using System.Linq;
using Apparatus.AOT.Reflection.Core.Stores;

namespace Apparatus.AOT.Reflection
{{
    public static class {typeToBake.ToSafeGlobalName()}Extensions
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            EnumMetadataStore<{typeGlobalName}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>>(new global::System.Collections.Generic.Dictionary<{typeGlobalName}, IEnumValueInfo<{typeGlobalName}>>
        {{
{typeToBake
    .GetMembers()
    .OfType<IFieldSymbol>()
    .Select(o => $@"
            {{ {typeGlobalName}.{o.Name}, new EnumValueInfo<{typeGlobalName}>(
                ""{o.Name}"", 
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
    }
}