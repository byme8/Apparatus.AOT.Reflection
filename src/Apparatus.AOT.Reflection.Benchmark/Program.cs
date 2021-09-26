using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Apparatus.AOT.Reflection.Playground;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Apparatus.AOT.Reflection.Benchmark
{
    public enum AccountKind
    {
        [Description("User account")]
        User,
        [Description("Admin account")]
        Admin,
        [Description("Customer account")]
        Customer,
        [Description("Manager account")]
        Manager
    }
    
    public static class EnumHelperReflection
    {
        public static string GetDescription<T>(this T enumValue) 
            where T : Enum
        {
            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(description);

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs is { Length: > 0 })
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }
    }
    
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<EnumBenchmark>();
        }
    }

    [MemoryDiagnoser()]
    public class EnumBenchmark
    {
        private readonly AccountKind _account;

        public EnumBenchmark()
        {
            _account = AccountKind.Customer;
        }
        
        [Benchmark]
        public string GetValuesAOT()
        {
            var attributes = _account.GetEnumValueInfo().Attributes;;
            for (int i = 0; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                if (attribute is DescriptionAttribute descriptionAttribute)
                {
                    return descriptionAttribute.Description;
                }
            }
            
            return "";
        }
        
        [Benchmark]
        public void GetValuesReflection()
        {
            var values = _account.GetDescription();
        }
    }

    [MemoryDiagnoser]
    public class PropertiesBenchmark
    {
        private readonly User _user;

        public PropertiesBenchmark()
        {
            _user = new User
            {
                FirstName = "Test",
            };
        }

        // [Benchmark]
        public string Direct()
        {
            return _user.FirstName;
        }

        [Benchmark]
        public string Reflection()
        {
            var type = _user.GetType();
            var property = type.GetProperty(nameof(User.FirstName));

            var required = false;
            foreach (var o in property.GetCustomAttributes())
            {
                if (o.GetType() == typeof(RequiredAttribute))
                {
                    required = true;
                    break;
                }
            }

            if (required)
            {
                return (string)property.GetMethod?.Invoke(_user, null);
            }

            return string.Empty;
        }

        [Benchmark]
        public string AOTReflection()
        {
            var entries = _user.GetProperties();
            var firstName = entries["LastName"];

            var required = false;
            foreach (var o in firstName.Attributes)
            {
                if (o is RequiredAttribute)
                {
                    required = true;
                    break;
                }
            }

            if (required)
            {
                if (firstName.TryGetValue(_user, out var value))
                {
                    return (string)value;
                }

                return string.Empty;
            }

            return string.Empty;
        }
    }
}