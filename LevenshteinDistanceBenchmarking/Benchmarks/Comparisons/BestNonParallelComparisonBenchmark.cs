using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Comparisons;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks.Comparisons
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class BestNonParallelComparisonBenchmark : TextBenchmarkBase
	{
		[Params(8, 128, 512, 2048, 8192)]
		public int NumberOfCharacters;

		[GlobalSetup]
		public void Setup()
		{
			InitialiseDefaultComparisonString(NumberOfCharacters);
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new BestNonParallelCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void Comparison()
		{
			new BestNonParallelCalculatorComparison()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
