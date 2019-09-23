using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	public abstract class TextBenchmarkBase
	{
		protected string ComparisonStringA;

		protected string ComparisonStringB;

		protected void InitialiseDefaultComparisonString(int numberOfCharacters)
		{
			ComparisonStringA = Utilities.BuildString("aabbccddee", numberOfCharacters);
			ComparisonStringB = Utilities.BuildString("abcdeabcde", numberOfCharacters);
		}
	}
}
