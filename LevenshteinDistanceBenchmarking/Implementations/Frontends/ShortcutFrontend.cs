using System;
using System.Collections.Generic;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Frontends
{
	public class ShortcutFrontend : ILevenshteinDistanceSpanCalculator
	{
		private readonly ILevenshteinDistanceSpanCalculator Calculator = new LevenshteinDistanceBaseline();

		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			if (source.Length == 0)
			{
				return target.Length;
			}

			if (target.Length == 0)
			{
				return source.Length;
			}

			var startIndex = 0;
			var sourceEnd = source.Length;
			var targetEnd = target.Length;

			while (startIndex < sourceEnd && startIndex < targetEnd && source[startIndex] == target[startIndex])
			{
				startIndex++;
			}
			while (startIndex < sourceEnd && startIndex < targetEnd && source[sourceEnd - 1] == target[targetEnd - 1])
			{
				sourceEnd--;
				targetEnd--;
			}

			var sourceLength = sourceEnd - startIndex;
			var targetLength = targetEnd - startIndex;

			if (sourceLength == 0)
			{
				return targetLength;
			}

			if (targetLength == 0)
			{
				return sourceLength;
			}

			return Calculator.CalculateDistance(
				source.Slice(startIndex, sourceLength),
				target.Slice(startIndex, targetLength)
			);
		}
	}
}
