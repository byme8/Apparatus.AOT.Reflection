using System;

namespace AttributesExtractor
{
    public interface IPropertyInfo
    {
        string Name { get; }
        AttributeData[] Attributes { get; }

        (object Value, bool Error) GetValue(object instance);
        bool SetValue(object instance, object value);
    }
    
    public class AttributeData
    {
        public AttributeData(Type type, params object[] parameters)
        {
            Type = type;
            Parameters = parameters;
        }

        public Type Type { get; set; }
        public object[] Parameters { get; set; }
    }
    
    public class PropertyInfo<TInstance, TPropertyType> : IPropertyInfo
    {
        private readonly Func<TInstance, TPropertyType> _getGetValue;
        private readonly Action<TInstance, TPropertyType> _setGetValue;
    
        public PropertyInfo(
            string name, 
            AttributeData[] attributes, 
            Func<TInstance, TPropertyType> getGetValue = null, 
            Action<TInstance, TPropertyType> setGetValue = null)
        {
            Name = name;
            Attributes = attributes;
            _getGetValue = getGetValue;
            _setGetValue = setGetValue;
        }

        public string Name { get; }
        public AttributeData[] Attributes { get; }


        public (object Value, bool Error) GetValue(object instance)
        {
            if (instance is TInstance typedInstance && _getGetValue != null)
            {
                return (_getGetValue.Invoke(typedInstance), false);
            }

            return (default, true);
        }

        public bool SetValue(object instance, object value)
        {
            if (instance is TInstance typedInstance &&
                value is TPropertyType propertyValue && 
                _setGetValue != null)
            {
                _setGetValue.Invoke(typedInstance, propertyValue);
                return true;
            }

            return false;
        }
    }

    public static class AttributesExtractorExtensions
    {
        public static IPropertyInfo[] GetAttributes<TValue>(this TValue value)
        {
            return null;
        }   
    } 
}