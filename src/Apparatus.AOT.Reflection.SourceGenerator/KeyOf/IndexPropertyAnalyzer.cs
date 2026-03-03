using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Apparatus.AOT.Reflection.SourceGenerator.KeyOf
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IndexPropertyAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterOperationAction(Handle, OperationKind.PropertyReference);
        }

        private void Handle(OperationAnalysisContext context)
        {
            if (context.Operation is not IPropertyReferenceOperation propertyReference)
            {
                return;
            }

            if (propertyReference.Arguments.IsEmpty)
            {
                return;
            }

            KeyOfAnalyzer.AnalyzeKeyOfUsages(
                context,
                propertyReference.Property.Parameters,
                propertyReference.Arguments);
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                DiagnosticDescriptors.TypeDoesntContainsPropertyWithSuchName,
                DiagnosticDescriptors.ImpossibleToGetThePropertyName);
    }
}
