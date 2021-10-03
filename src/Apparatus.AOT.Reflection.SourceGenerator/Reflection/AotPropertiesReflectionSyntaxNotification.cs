using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Apparatus.AOT.Reflection.SourceGenerator.Reflection
{
    public class AotPropertiesReflectionSyntaxNotification : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Invocations { get; } = new List<InvocationExpressionSyntax>();
        public List<GenericNameSyntax> GenericNames { get; } = new List<GenericNameSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation)
            {
                Invocations.Add(invocation);
            }

            if (syntaxNode is GenericNameSyntax genericNameSyntax && 
                genericNameSyntax.Identifier.Text == "KeyOf")
            {
                GenericNames.Add(genericNameSyntax);
            }
        }
    }
}