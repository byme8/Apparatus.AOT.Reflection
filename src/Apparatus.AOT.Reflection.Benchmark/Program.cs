using System;
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
        User,
        Admin,
        Customer,
        Manager
    }
    
    public class Program
    {
        public static void Main()
        {
            
            // new PropertiesBenchmark().Reflection();
            // new PropertiesBenchmark().AttributeExtractor();

            BenchmarkRunner.Run<EnumBenchmark>();
        }
    }

    [MemoryDiagnoser()]
    public class EnumBenchmark
    {
        [Benchmark]
        public void GetValuesAOT()
        {
            var values = EnumHelper.GetEnumInfo<AccountKind>();
        }
        
        [Benchmark]
        public void GetValuesReflection()
        {
            var values = Enum.GetValues<AccountKind>();
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
            var firstName = entries[nameof(User.FirstName)];

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