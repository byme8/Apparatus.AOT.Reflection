using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Apparatus.AOT.Reflection.SourceGenerator
{
    public static class Utils
    {
        public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol symbol)
        {
            if (symbol.BaseType != null)
            {
                foreach (var member in symbol.BaseType.GetAllMembers())
                {
                    yield return member;
                }
            }

            foreach (var member in symbol.GetMembers())
            {
                yield return member;
            }
        }

        public static ITypeSymbol GetTypeSymbol(this SymbolInfo info)
        {
            switch (info.Symbol)
            {
                case ITypeSymbol type:
                    return type;
                case ILocalSymbol local:
                    return local.Type;
                case IParameterSymbol parameterSymbol:
                    return parameterSymbol.Type;
                default:
                    return null;
            }
        }

        public static IEnumerable<TSymbol> GetPublicMembers<TSymbol>(this IEnumerable<ISymbol> members)
            where TSymbol: ISymbol
        {
            return members
                .OfType<TSymbol>()
                .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public));
        }
        
        public static string GenerateAttributes(this ImmutableArray<AttributeData> attributes)
        {
            return attributes
                .Select(o =>
                {
                    var parameters = o.AttributeConstructor.Parameters
                        .Select((parameter, i) =>
                            new KeyValuePair<string, TypedConstant>(parameter.Name, o.ConstructorArguments[i]))
                        .Select(Convert);

                    return
                        $@"new {o.AttributeClass.ToGlobalName()}({parameters.Join()}),";
                })
                .JoinWithNewLine();
        }

        public  static string Convert(KeyValuePair<string, TypedConstant> pair)
        {
            if (pair.Value.Kind == TypedConstantKind.Array && !pair.Value.IsNull)
            {
                return $@"new[] {pair.Value.ToCSharpString()}";
            }

            return $@"{pair.Value.ToCSharpString()}";
        }

        public static string GetUniqueName(this ITypeSymbol type)
        {
            return $"{type.Name}_{Guid.NewGuid().ToString().Replace("-", "")}";
        }

        public static SourceText ToSourceText(this string source)
        {
            return SourceText.From(source, Encoding.UTF8);
        }

        public static string ToGlobalName(this ISymbol symbol)
        {
            return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        public static string ToFileName(this ISymbol symbol)
        {
            return symbol.ToGlobalName().Replace(".", "_").Replace("global::", "") + "Extensions";
        }

        public static string Join(this IEnumerable<string> values, string separator = ", ")
        {
            return string.Join(separator, values);
        }

        public static string Wrap(this string text, string left = "", string right = "")
        {
            return $"{left}{text}{right}";
        }

        public static string JoinWithNewLine(this IEnumerable<string> values, string separator = "")
        {
            return string.Join($"{separator}{Environment.NewLine}", values);
        }
    }
}