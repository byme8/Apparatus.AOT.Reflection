using System;
using System.Linq;

namespace Apparatus.AOT.Reflection
{
    public interface IEnumValueInfo
    {
        string Name { get; }
        Attribute[] Attributes { get; }

        int Value { get; }
    }

    public class EnumValueInfo : IEnumValueInfo, IEquatable<EnumValueInfo>
    {
        public bool Equals(EnumValueInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name && Value == other.Value && Attributes.SequenceEqual(other.Attributes);
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

            return Equals((EnumValueInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Value;
                hashCode = (hashCode * 397) ^ (Attributes != null ? Attributes.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EnumValueInfo left, EnumValueInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EnumValueInfo left, EnumValueInfo right)
        {
            return !Equals(left, right);
        }

        public EnumValueInfo(string name, int value, Attribute[] attributes)
        {
            Name = name;
            Attributes = attributes;
            Value = value;
        }

        public string Name { get; }
        public int Value { get; }
        public Attribute[] Attributes { get; }
    }
}