﻿using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements;
using LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements.DataStructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class ArrayPoolVariationBenchmark : TextBenchmarkBase
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
		public void ArrayPoolDenseMatrix()
		{
			new ArrayPoolDenseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void SingleLoopArrayPoolDenseMatrix()
		{
			new SingleLoopArrayPoolDenseMatrixCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
