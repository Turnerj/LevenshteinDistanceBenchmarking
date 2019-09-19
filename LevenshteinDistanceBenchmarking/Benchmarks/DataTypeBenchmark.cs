using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Alternatives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class DataTypeBenchmark : TextBenchmarkBase
	{
		[Params(8, 128, 512, 2048, 8192)]
		public int NumberOfCharacters;

		[GlobalSetup]
		public void Setup()
		{
			InitialiseComparisonString(NumberOfCharacters);
		}

		[Benchmark(Baseline = true)]
		public void Baseline()
		{
			new LevenshteinDistanceBaseline()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}

		[Benchmark]
		public void ShortMatrix()
		{
			new ShortMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void FloatMatrix()
		{
			new FloatMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
