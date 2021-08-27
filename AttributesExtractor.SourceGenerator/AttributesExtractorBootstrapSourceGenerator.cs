using Microsoft.CodeAnalysis;

namespace AttributesExtractor.SourceGenerator
{
    [Generator]
    public class AttributesExtractorBootstrapSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("AttributesExtractorBootstrap", @"
using System;

namespace AttributesExtractor 
{
    public class Entry
    {
        public Entry(string propertyName, Attribute[] attributes)
        {
            PropertyName = propertyName;
            Attributes = attributes; 
        }

        public string PropertyName { get; }
        public Attribute[] Attributes { get; }
    }

    public static class AttributesExtractorExtensions
    {
        public static Entry[] GetAttributes<TValue>(this TValue value)
        {
            return null;
        }   
    } 
}
");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}