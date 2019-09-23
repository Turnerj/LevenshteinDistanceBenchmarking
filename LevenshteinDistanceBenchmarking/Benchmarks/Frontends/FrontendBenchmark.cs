using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Frontends;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks.Frontends
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class FrontendBenchmark : TextBenchmarkBase
	{
		[ParamsSource(nameof(TestStringValuesA))]
		public string TestStringA;
		[ParamsSource(nameof(TestStringValuesB))]
		public string TestStringB;

		public static IEnumerable<string> TestStringValuesA()
		{
			yield return "Hello, this is a test world!";
			yield return "NOT A TEST";
			yield return "";
			yield return BuildString("a", 100);
		}
		public static IEnumerable<string> TestStringValuesB()
		{
			yield return "Hello world!";
			yield return "TEST";
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
		public void ShortcutFrontend()
		{
			new ShortcutFrontend()
				.CalculateDistance(TestStringA, TestStringB);
		}
	}
}
