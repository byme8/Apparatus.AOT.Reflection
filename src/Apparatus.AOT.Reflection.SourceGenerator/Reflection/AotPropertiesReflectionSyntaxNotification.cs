using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    public class AotPropertiesReflectionSyntaxNotification : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Invocations { get; } = new List<InvocationExpressionSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation)
            {
                Invocations.Add(invocation);
            }
        }
    }
}