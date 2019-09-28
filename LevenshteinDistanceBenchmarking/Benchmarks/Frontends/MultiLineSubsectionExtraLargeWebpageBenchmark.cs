using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Frontends;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks.Frontends
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class MultiLineSubsectionExtraLargeWebpageBenchmark : TextBenchmarkBase
	{
		public string TestStringA;
		public string TestStringB;

		[GlobalSetup]
		public void Setup()
		{
			TestStringA = Utilities.ReadTestData("ExtraLargeWebpage1a.html");
			TestStringB = Utilities.ReadTestData("ExtraLargeWebpage1b.html");
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new BestNonParallelCalculator()
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
