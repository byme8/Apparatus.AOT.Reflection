using BenchmarkDotNet.Running;

namespace Apparatus.AOT.Reflection.Benchmark
{
    public class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<PropertiesBenchmark>();
            BenchmarkRunner.Run<EnumBenchmark>();
            BenchmarkRunner.Run<IntToEnumBenchmark>();
            BenchmarkRunner.Run<EnumToIntBenchmark>();
        }
    }
}