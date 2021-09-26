using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    public class AotPropertiesReflectionSyntaxNotification : ISyntaxReceiver
    {
        public List<MemberAccessExpressionSyntax> MemberAccess { get; } = new List<MemberAccessExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                (memberAccess.Name.ToString() == "GetProperties" ||
                 memberAccess.Name is GenericNameSyntax genericNameSyntax &&
                 genericNameSyntax.Identifier.ToString() == "Bootstrap"))
            {
                MemberAccess.Add(memberAccess);
            }
        }
    }
}