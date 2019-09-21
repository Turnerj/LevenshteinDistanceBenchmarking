using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Alternatives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class BestOfBenchmark : TextBenchmarkBase
	{
		[Params(8, 512, 8192)]//, 32768, 131072)]
		public int NumberOfCharacters;

		[GlobalSetup]
		public void Setup()
		{
			InitialiseDefaultComparisonString(NumberOfCharacters);
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new LevenshteinDistanceBaseline()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void BestNonParallel()
		{
			new BestNonParallelCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void BestNonParallelIntrinsic()
		{
			new BestNonParallelIntrinsicCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void BestParallel()
		{
			new BestParallelCalculator()
				.CalculateDistance(ComparisonStringA.AsMemory(), ComparisonStringB.AsMemory());
		}
	}
}
