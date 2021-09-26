using System.ComponentModel;

namespace Apparatus.AOT.Reflection
{
    public class KeyOf<T>
    {
        private KeyOf(string value)
        {
            
            Value = value;
        }

        public string Value { get; }

        public static implicit operator KeyOf<T>(string property)
        {
            return new KeyOf<T>(property);
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static KeyOf<T> Parse(string property)
        {
            return new KeyOf<T>(property);
        }
    }
}