using System;

namespace Apparatus.AOT.Reflection
{
    public class KeyOf<T> : IEquatable<KeyOf<T>>
    {
        public KeyOf(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public bool Equals(KeyOf<T> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Value == other.Value;
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

            return Equals((KeyOf<T>)obj);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public static bool operator ==(KeyOf<T> left, KeyOf<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KeyOf<T> left, KeyOf<T> right)
        {
            return !Equals(left, right);
        }

        public static implicit operator KeyOf<T>(string property)
        {
            return new KeyOf<T>(property);
        }

        public static bool TryParse(string property, out KeyOf<T> key)
        {
            var maybeKeyOf = new KeyOf<T>(property);
            if (MetadataStore<T>.Data.Value.ContainsKey(maybeKeyOf))
            {
                key = maybeKeyOf;
                return true;
            }

            key = null;
            return false;
        }
    }
}