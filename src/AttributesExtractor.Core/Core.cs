using System;

namespace AttributesExtractor
{
    public class Entry
    {
        public Entry(string propertyName, EntryAttribute[] attributes)
        {
            PropertyName = propertyName;
            Attributes = attributes; 
        }

        public string PropertyName { get; }
        public EntryAttribute[] Attributes { get; }
        
        public class EntryAttribute
        {
            public EntryAttribute(Type type, params object[] parameters)
            {
                Type = type;
                Parameters = parameters;
            }

            public Type Type { get; set; }
            public object[] Parameters { get; set; }
        }
    }

    public static class AttributesExtractorExtensions
    {
        public static Entry[] GetAttributes<TValue>(this TValue value)
        {
            return null;
        }   
    } 
}