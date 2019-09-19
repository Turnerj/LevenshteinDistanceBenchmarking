using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	interface ILevenshteinDistanceCalculator
	{
		int CalculateDistance(string source, string target);
	}
}
