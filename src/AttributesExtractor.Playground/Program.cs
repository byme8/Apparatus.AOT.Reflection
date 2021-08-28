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

        public static IPropertyInfo[] GetUserInfo()
        {
            return GetInfo(new User());
        }
        
        public static IPropertyInfo[] GetInfo<T>(T value)
        {
            return value.GetProperties();
        } 
    }
}