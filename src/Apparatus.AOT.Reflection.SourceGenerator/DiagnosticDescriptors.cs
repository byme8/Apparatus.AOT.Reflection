using Microsoft.CodeAnalysis;

namespace Apparatus.AOT.Reflection.SourceGenerator
{
    public class DiagnosticDescriptors
    {
        public static readonly DiagnosticDescriptor EnumDoesntHaveProperties = new DiagnosticDescriptor(
            "AOTRF_001",
            "The enums do not have properties", 
            "The {0} is enum and enums do not have properties", 
            "Reflection",
            DiagnosticSeverity.Error,
            true
        );
        
        public static readonly DiagnosticDescriptor TypeDoesntContainsPropertyWithSuchName = new DiagnosticDescriptor(
            "KEYOF_001",
            "The property is missing in the type", 
            "The property '{0}' is missing in the type '{1}'", 
            "KeyOf",
            DiagnosticSeverity.Error,
            true
        );
    }
}