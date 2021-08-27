using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AttributesExtractor.SourceGenerator
{
    [Generator]
    public class AttributesExtractorSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is AttributesExtractorSyntaxNotification receiver))
            {
                return;
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new AttributesExtractorSyntaxNotification());
        }
    }

    public class AttributesExtractorSyntaxNotification : ISyntaxReceiver
    {
        public List<InvocationExpressionSyntax> Invocations { get; } = new List<InvocationExpressionSyntax>();
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.ToString() == "GetAttributes")
            {
                Invocations.Add(invocation);
            }
        }
    }
}
