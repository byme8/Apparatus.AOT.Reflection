using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AttributesExtractor.SourceGenerator
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

        public static IEnumerable<IMethodSymbol> GetPublicMethods(this IEnumerable<ISymbol> members)
        {
            return members
                .OfType<IMethodSymbol>()
                .Where(o => o.DeclaredAccessibility == Accessibility.Public)
                .Where(o => o.MethodKind == MethodKind.Ordinary);
        }

        public static (bool IsDuckable, IEnumerable<ISymbol> MissingSymbols) IsTypeDuckableTo(
            this ITypeSymbol @interface, ITypeSymbol implementation)
        {
            var methodsToDuck = MemberThatCanBeDucked(@interface);
            var memberThatCanBeDucked = MemberThatCanBeDucked(implementation);

            var missingSymbols = methodsToDuck
                .Where(o => !memberThatCanBeDucked.ContainsKey(o.Key))
                .Select(o => o.Value)
                .ToArray();

            return (!missingSymbols.Any(), missingSymbols);
        }

        private static Dictionary<string, ISymbol> MemberThatCanBeDucked(ITypeSymbol type)
        {
            return type
                .GetAllMembers()
                .GetPublicMethods()
                .Select(o =>
                (
                    Key:
                    o.ReturnType.ToGlobalName() +
                    o.Name +
                    o.Parameters
                        .Select(oo => oo.Type.ToGlobalName() + oo.Name)
                        .Join(),
                    Value: (ISymbol)o
                ))
                .Concat(type
                    .GetAllMembers()
                    .OfType<IPropertySymbol>()
                    .Where(o => o.DeclaredAccessibility.HasFlag(Accessibility.Public))
                    .Select(o =>
                    (
                        Key: o.Type.ToGlobalName() + o.Name + (o.GetMethod != null ? "getter" : string.Empty) +
                             (o.SetMethod != null ? "setter" : string.Empty),
                        Value: (ISymbol)o
                    )))
                .ToDictionary(o => o.Key, o => o.Value);
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