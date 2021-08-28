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
            BenchmarkRunner.Run<AttributeBenchmark>();
        }
    }

    [MemoryDiagnoser]
    public class AttributeBenchmark
    {
        private readonly User _user;

        public AttributeBenchmark()
        {
            _user = new User();
        }
        
        [Benchmark]
        public bool Reflection()
        {
            var type = _user.GetType();
            var property = type.GetProperty(nameof(User.FirstName));

            var result = property.GetCustomAttributes().Any(o => o.GetType() == typeof(RequiredAttribute));
            
            return result;
        }
        
        [Benchmark]
        public bool AttributeExtractor()
        {
            var entries = _user.GetProperties();
            var firstName = entries.First(o => o.Name == nameof(User.FirstName));

            var result = firstName.Attributes.Any(o => o.Type == typeof(RequiredAttribute));
            return result;
        }
    }
}