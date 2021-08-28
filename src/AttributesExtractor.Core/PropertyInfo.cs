using System;

namespace AttributesExtractor
{
    public interface IPropertyInfo
    {
        string Name { get; }
        AttributeData[] Attributes { get; }

        bool TryGetValue(object instance, out object value);
        bool TrySetValue(object instance, object value);
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


        public bool TryGetValue(object instance, out object value)
        {
            if (instance is TInstance typedInstance && _getGetValue != null)
            {
                value = _getGetValue.Invoke(typedInstance);
                return true;
            }

            value = null;
            return false;
        }

        public bool TrySetValue(object instance, object value)
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
}