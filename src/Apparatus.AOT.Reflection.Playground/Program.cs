using System;
using System.Collections.Generic;

namespace Apparatus.AOT.Reflection.Playground
{
    // place to replace 0

    public static class Program
    {
        // place to replace properties

        private static void Main(string[] args)
        {
            var user = new User(); // 1
            // place to replace 1
        }

        private static void DontCall()
        {
            var user = new User(); // 2
            var attributes = user.GetProperties();
            var userKind = UserKind.Admin;
            var value = userKind.GetEnumValueInfo();
        }

        public static IReadOnlyDictionary<KeyOf<User>, IPropertyInfo> GetUserInfo()
        {
            return GetInfo(new User());
        }

        public static IReadOnlyDictionary<KeyOf<T>, IPropertyInfo> GetInfo<T>(T value)
        {
            return value.GetProperties();
        }

        public static IEnumValueInfo<TEnum> GetEnumValueInfo<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return value.GetEnumValueInfo();
        }

        public static IEnumerable<IEnumValueInfo<TEnum>> GetEnumInfo<TEnum>()
            where TEnum : Enum
        {
            return EnumHelper.GetEnumInfo<TEnum>();
        }
    }
}