using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Frontends;
using LevenshteinDistanceBenchmarking.Implementations.Frontends.Comparisons;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks.Frontends
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class MultiLineSubsectionComparisonBenchmark : TextBenchmarkBase
	{
		[ParamsSource(nameof(TestStringValuesA))]
		public string TestStringA;
		[ParamsSource(nameof(TestStringValuesB))]
		public string TestStringB;

		public static IEnumerable<string> TestStringValuesA()
		{
			yield return Utilities.ReadTestData("MultilineLipsum1a.txt");
			yield return Utilities.BuildString("a", 100);
			yield return Utilities.ReadTestData("TestHtml1a.html");
		}
		public static IEnumerable<string> TestStringValuesB()
		{
			yield return Utilities.ReadTestData("MultilineLipsum1b.txt");
			yield return Utilities.BuildString("a", 100);
			yield return Utilities.ReadTestData("TestHtml1b.html");
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new MultiLineSubsectionFrontend()
				.CalculateDistance(TestStringA, TestStringB);
		}

		[Benchmark]
		public void Comparison()
		{
			new MultiLineSubsectionFrontendComparison()
				.CalculateDistance(TestStringA, TestStringB);
		}
	}
}
