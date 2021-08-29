using System.Collections.Generic;
using System.Linq;

namespace AttributesExtractor.Playground
{

    // place to replace 0

    static class Program
    {
        static void Main(string[] args)
        {
            var user = new User();
            // place to replace 1
        }

        static void DontCall()
        {
            var user = new User();
            var attributes = user.GetProperties();
        }

        public static global::System.Collections.Generic.IReadOnlyDictionary<string, IPropertyInfo> GetUserInfo()
        {
            return GetInfo(new User());
        }
        
        public static global::System.Collections.Generic.IReadOnlyDictionary<string, IPropertyInfo> GetInfo<T>(T value)
        {
            return value.GetProperties();
        } 
    }
}