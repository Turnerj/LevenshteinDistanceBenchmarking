using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Alternatives;
using LevenshteinDistanceBenchmarking.Implementations.Comparisons;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class BestParallelComparisonBenchmark : TextBenchmarkBase
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
			new BestParallelCalculator()
				.CalculateDistance(ComparisonStringA.AsMemory(), ComparisonStringB.AsMemory());
		}

		[Benchmark]
		public void Comparison()
		{
			new BestParallelCalculatorComparison()
				.CalculateDistance(ComparisonStringA.AsMemory(), ComparisonStringB.AsMemory());
		}
	}
}
