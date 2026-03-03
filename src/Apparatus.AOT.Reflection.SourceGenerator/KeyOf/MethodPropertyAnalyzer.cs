using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MethodPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterOperationAction(Handle, OperationKind.Invocation);
        }

        private void Handle(OperationAnalysisContext context)
        {
            if (!(context.Operation is IInvocationOperation invocation))
            {
                return;
            }

            KeyOfAnalyzer.AnalyzeKeyOfUsages(
                context,
                invocation.TargetMethod.Parameters,
                invocation.Arguments);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName,
                DiagnosticDescriptors.ImpossibleToGetThePropertyName);
    }
}
