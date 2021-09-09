using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator
{
    public class AotEnumReflectionSyntaxNotification : ISyntaxReceiver
    {
        private static HashSet<String> MethodsNames = new HashSet<string>
        {
            "GetEnumValueInfo", 
            "GetEnumInfo"
        };

        public List<MemberAccessExpressionSyntax> MemberAccess { get; } = new List<MemberAccessExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (MethodsNames.Contains(memberAccess.Name.ToString()) ||
                 memberAccess.Name is GenericNameSyntax genericNameSyntax &&
                 genericNameSyntax.Identifier.ToString() == "Bootstrap"))
            {
                MemberAccess.Add(memberAccess);
            }
        }
    }
}