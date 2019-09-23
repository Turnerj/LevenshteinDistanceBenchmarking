using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	public interface ILevenshteinDistanceCalculator
	{

	}

	public interface ILevenshteinDistanceSpanCalculator : ILevenshteinDistanceCalculator
	{
		int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target);
	}

	public interface ILevenshteinDistanceMemoryCalculator : ILevenshteinDistanceCalculator
	{
		int CalculateDistance(ReadOnlyMemory<char> source, ReadOnlyMemory<char> target);
	}
}
