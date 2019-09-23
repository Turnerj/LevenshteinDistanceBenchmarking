using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class LocalVariablesBenchmark : TextBenchmarkBase
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
			new LevenshteinDistanceBaseline()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void LocalRowData()
		{
			new LocalRowDataCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void LocalStringLength()
		{
			new LocalStringLengthCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void LocalLastInsert()
		{
			new LocalLastInsertCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
