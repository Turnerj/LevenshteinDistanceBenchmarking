using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Frontends;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class LineEqualityBenchmark : TextBenchmarkBase
	{
		[ParamsSource(nameof(TestStringValuesA))]
		public string TestStringA;
		[ParamsSource(nameof(TestStringValuesB))]
		public string TestStringB;

		public static IEnumerable<string> TestStringValuesA()
		{
			yield return FromTestData("MultilineLipsum1a.txt");
			yield return "";
			yield return BuildString("a", 100);
		}
		public static IEnumerable<string> TestStringValuesB()
		{
			yield return FromTestData("MultilineLipsum1b.txt");
			yield return "";
			yield return BuildString("a", 100);
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new LevenshteinDistanceBaseline()
				.CalculateDistance(TestStringA, TestStringB);
		}

		[Benchmark]
		public void LineEquality()
		{
			new LineEqualityFrontend()
				.CalculateDistance(TestStringA, TestStringB);
		}
	}
}
