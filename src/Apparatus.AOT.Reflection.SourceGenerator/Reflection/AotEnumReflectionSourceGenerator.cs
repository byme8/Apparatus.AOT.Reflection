using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    [Generator]
    public class AotEnumReflectionSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AotEnumReflectionSyntaxNotification());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AotEnumReflectionSyntaxNotification receiver))
            {
                return;
            }

            if (!receiver.MemberAccess.Any())
            {
                return;
            }
            
            var extensionType = context.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.AOTReflectionExtensions");
            var extensionMethod = extensionType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .First(o => o.Name == "GetEnumValueInfo");

            var enumHelper = context.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.EnumHelper");
            var enumHelperMethod = enumHelper
                .GetMembers()
                .OfType<IMethodSymbol>()
                .First(o => o.Name == "GetEnumInfo");

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
                    !SymbolEqualityComparer.Default.Equals(enumHelperMethod, methodSymbol.ConstructedFrom))
                {
                    continue;
                }


                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }


                var typeToBake = methodSymbol.TypeArguments.First();
                if (processed.Contains(typeToBake.ToGlobalName()))
                {
                    continue;
                }

                if (typeToBake is ITypeParameterSymbol)
                {
                    continue;
                }

                if (typeToBake.TypeKind != TypeKind.Enum)
                {
                    continue;
                }

                var source = GenerateExtensionForEnum(typeToBake);
                if (context.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                context.AddSource(typeToBake.ToFileName(), source);
                processed.Add(typeToBake.ToGlobalName());
            }
        }

        private string GenerateExtensionForEnum(ITypeSymbol typeToBake)
        {
            var source = $@"
using System;
using System.Linq;

namespace Apparatus.AOT.Reflection
{{
    public static class {typeToBake.ToFileName()}
    {{
        [global::System.Runtime.CompilerServices.ModuleInitializer]
        public static void Bootstrap()
        {{
            EnumMetadataStore<{typeToBake.ToGlobalName()}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeToBake.ToGlobalName()}, IEnumValueInfo<{typeToBake.ToGlobalName()}>>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<{typeToBake.ToGlobalName()}, IEnumValueInfo<{typeToBake.ToGlobalName()}>>>(new global::System.Collections.Generic.Dictionary<{typeToBake.ToGlobalName()}, IEnumValueInfo<{typeToBake.ToGlobalName()}>>
        {{
{typeToBake
    .GetMembers()
    .OfType<IFieldSymbol>()
    .Select(o => 
$@"         {{ {typeToBake.ToGlobalName()}.{o.Name}, new EnumValueInfo<{typeToBake.ToGlobalName()}>(""{o.Name}"", {typeToBake.ToGlobalName()}.{o.Name}, new Attribute[] 
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