using System.ComponentModel.DataAnnotations;
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
                if (o.GetType() == typeof(RequiredAttribute))
                {
                    required = true;
                    break;
                }

            if (required) return (string)property.GetMethod?.Invoke(_user, null);

            return string.Empty;
        }

        [Benchmark]
        public string AttributeExtractor()
        {
            var entries = _user.GetProperties();
            var firstName = entries[nameof(User.FirstName)];

            var required = false;
            foreach (var o in firstName.Attributes)
                if (o.Type == typeof(RequiredAttribute))
                {
                    required = true;
                    break;
                }

            if (required)
            {
                if (firstName.TryGetValue(_user, out var value)) return (string)value;

                return string.Empty;
            }

            return string.Empty;
        }
    }
}