using System;
using System.Linq;

namespace Apparatus.AOT.Reflection
{
    public interface IPropertyInfo
    {
        string Name { get; }
        Attribute[] Attributes { get; }
        Type PropertyType { get; }

        bool TryGetValue(object instance, out object value);
        bool TrySetValue(object instance, object value);
    }

    public class PropertyInfo<TInstance, TPropertyType> : IPropertyInfo, IEquatable<PropertyInfo<TInstance, TPropertyType>>
    {
        public bool Equals(PropertyInfo<TInstance, TPropertyType> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(_getGetValue, other._getGetValue) && 
                   Equals(_setGetValue, other._setGetValue) && Name == other.Name && 
                   Attributes.SequenceEqual(other.Attributes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            var other = (PropertyInfo<TInstance, TPropertyType>)obj;
            return this.Name == other.Name && Attributes.SequenceEqual(other.Attributes);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_getGetValue != null ? _getGetValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_setGetValue != null ? _setGetValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(PropertyInfo<TInstance, TPropertyType> left, PropertyInfo<TInstance, TPropertyType> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyInfo<TInstance, TPropertyType> left, PropertyInfo<TInstance, TPropertyType> right)
        {
            return !Equals(left, right);
        }

        private readonly Func<TInstance, TPropertyType> _getGetValue;
        private readonly Action<TInstance, TPropertyType> _setGetValue;

        public PropertyInfo(
            string name,
            Attribute[] attributes,
            Func<TInstance, TPropertyType> getGetValue = null,
            Action<TInstance, TPropertyType> setGetValue = null)
        {
            Name = name;
            Attributes = attributes;
            PropertyType = typeof(TPropertyType);
            _getGetValue = getGetValue;
            _setGetValue = setGetValue;
        }

        public string Name { get; }
        public Attribute[] Attributes { get; }
        public Type PropertyType { get; }


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