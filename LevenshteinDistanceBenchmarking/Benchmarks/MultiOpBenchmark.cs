﻿using BenchmarkDotNet.Attributes;
using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements;
using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	[CoreJob, MemoryDiagnoser, MaxColumn]
	public class MultiOpBenchmark : TextBenchmarkBase
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
		public void DoubleOp()
		{
			new DoubleOpCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void QuadOp()
		{
			new QuadOpCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
		[Benchmark]
		public void QuadOpIntrinsic()
		{
			new QuadOpIntrinsicCalculator()
				.CalculateDistance(ComparisonStringA, ComparisonStringB);
		}
	}
}
