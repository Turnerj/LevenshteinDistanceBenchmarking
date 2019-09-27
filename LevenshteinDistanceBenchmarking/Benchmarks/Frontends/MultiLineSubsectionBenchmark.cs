using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Frontends;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks.Frontends
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class MultiLineSubsectionBenchmark : TextBenchmarkBase
	{
		[ParamsSource(nameof(TestStringValuesA))]
		public string TestStringA;
		[ParamsSource(nameof(TestStringValuesB))]
		public string TestStringB;

		public static IEnumerable<string> TestStringValuesA()
		{
			yield return Utilities.ReadTestData("MultilineLipsum1a.txt");
			yield return "";
			yield return Utilities.BuildString("a", 100);
		}
		public static IEnumerable<string> TestStringValuesB()
		{
			yield return Utilities.ReadTestData("MultilineLipsum1b.txt");
			yield return "";
			yield return Utilities.BuildString("a", 100);
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new LevenshteinDistanceBaseline()
				.CalculateDistance(TestStringA, TestStringB);
		}

		[Benchmark]
		public void MultiLineSubsection()
		{
			new MultiLineSubsectionFrontend()
				.CalculateDistance(TestStringA.AsMemory(), TestStringB.AsMemory());
		}
	}
}
