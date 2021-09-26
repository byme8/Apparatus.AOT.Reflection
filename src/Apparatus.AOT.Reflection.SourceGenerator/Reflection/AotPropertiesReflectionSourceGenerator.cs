using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apparatus.AOT.Reflection.SourceGenerator.KeyOf;
using Microsoft.CodeAnalysis;

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
            var extensionMethod = extensionType
                .GetMembers()
                .OfType<IMethodSymbol>()
                .First(o => o.Name == "GetProperties");
            
            var genericHelperType = context.Compilation
                .GetTypeByMetadataName("Apparatus.AOT.Reflection.GenericHelper");
            var bootstrapMethod =
                genericHelperType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .First(o => o.Name == "Bootstrap");

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
                    !SymbolEqualityComparer.Default.Equals(bootstrapMethod, methodSymbol.ConstructedFrom))
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
                        return o.Last();
                    }

                    return o.First();
                })
                .ToArray();

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
            MetadataStore<{typeToBake.ToGlobalName()}>.Data = _lazy;
        }}

        private static global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeToBake.ToGlobalName()}>, IPropertyInfo>> _lazy = new global::System.Lazy<global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeToBake.ToGlobalName()}>, IPropertyInfo>>(new global::System.Collections.Generic.Dictionary<KeyOf<{typeToBake.ToGlobalName()}>, IPropertyInfo>
        {{
{propertyAndAttributes.Select(o =>
        $@"            {{ new KeyOf<{typeToBake.ToGlobalName()}>(""{o.Name}""), new global::Apparatus.AOT.Reflection.PropertyInfo<{typeToBake.ToGlobalName()},{o.Type.ToGlobalName()}>(
                        ""{o.Name}"", 
                        new global::System.Attribute[] 
                        {{
                            {o.GetAttributes().GenerateAttributes()}
                        }}, 
                        {GenerateGetterAndSetter(o)})
                }},")
    .JoinWithNewLine()}
        }}); 


        {GetVisibility(typeToBake)} static global::System.Collections.Generic.IReadOnlyDictionary<KeyOf<{typeToBake.ToGlobalName()}>, IPropertyInfo> GetProperties(this {typeToBake.ToGlobalName()} value)
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