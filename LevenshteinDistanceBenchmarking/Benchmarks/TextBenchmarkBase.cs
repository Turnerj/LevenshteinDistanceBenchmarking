using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Benchmarks
{
	public abstract class TextBenchmarkBase
	{
		protected string ComparisonStringA;

		protected string ComparisonStringB;

		protected void InitialiseDefaultComparisonString(int numberOfCharacters)
		{
			ComparisonStringA = BuildString("aabbccddee", numberOfCharacters);
			ComparisonStringB = BuildString("abcdeabcde", numberOfCharacters);
		}

		protected static string BuildString(string baseString, int numberOfCharacters)
		{
			var builder = new StringBuilder(numberOfCharacters);
			var charBlocks = (int)Math.Floor((double)numberOfCharacters / baseString.Length);
			for (int i = 0, l = charBlocks; i < l; i++)
			{
				builder.Append(baseString);
			}

			var remainder = (int)((double)numberOfCharacters / baseString.Length % 1 * baseString.Length);
			builder.Append(baseString.Substring(0, remainder));

			return builder.ToString();
		}
	}
}
