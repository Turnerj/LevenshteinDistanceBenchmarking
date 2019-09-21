using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Alternatives;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class DataStructureBenchmark : TextBenchmarkBase
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
		public void DenseMatrix()
		{
			new DenseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void ArrayPoolDenseMatrix()
		{
			new ArrayPoolDenseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void MultiDimensionMatrix()
		{
			new MultiDimensionMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void TwoRowModulusMatrix()
		{
			new TwoRowModulusMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void TwoRowBitwiseMatrix()
		{
			new TwoRowBitwiseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void UnmanagedDenseMatrix()
		{
			new UnmanagedDenseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void SingleRow()
		{
			new SingleRowCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
