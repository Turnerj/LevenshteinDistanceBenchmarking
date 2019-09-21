using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	interface ILevenshteinDistanceCalculator
	{

	}

	interface ILevenshteinDistanceSpanCalculator : ILevenshteinDistanceCalculator
	{
		int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target);
	}

	interface ILevenshteinDistanceMemoryCalculator : ILevenshteinDistanceCalculator
	{
		int CalculateDistance(ReadOnlyMemory<char> source, ReadOnlyMemory<char> target);
	}
}
