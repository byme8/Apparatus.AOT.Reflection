using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using AttributesExtractor.Playground;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AttributesExtractor.Benchmark
{
    public class Program
    {
        public static void Main()
        {
            // new AttributeBenchmark().Reflection();
            // new AttributeBenchmark().AttributeExtractor();
            
            BenchmarkRunner.Run<AttributeBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class AttributeBenchmark
    {
        private readonly User _user;

        public AttributeBenchmark()
        {
            _user = new User
            {
                FirstName = "Test"
            };
        }

        [Benchmark]
        public string Direct()
        {
            return _user.FirstName;
        }
        
        [Benchmark]
        public string Reflection()
        {
            var type = _user.GetType();
            var property = type.GetProperty(nameof(User.FirstName));

            var required = property.GetCustomAttributes().Any(o => o.GetType() == typeof(RequiredAttribute));
            if (required)
            {
                return (string)property.GetMethod?.Invoke(_user, null);
            }

            return string.Empty;
        }

        [Benchmark]
        public string AttributeExtractor()
        {
            var entries = _user.GetProperties();
            var firstName = entries.First(o => o.Name == nameof(User.FirstName));

            var required = firstName.Attributes.Any(o => o.Type == typeof(RequiredAttribute));
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