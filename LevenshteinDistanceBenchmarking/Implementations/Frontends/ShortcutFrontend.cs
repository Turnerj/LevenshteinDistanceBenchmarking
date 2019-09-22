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
			//Shortcut any processing if either string is empty
			if (source.Length == 0)
			{
				return target.Length;
			}
			if (target.Length == 0)
			{
				return source.Length;
			}

			//Identify and trim any common prefix or suffix between the strings
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

			//Check the trimmed values are not empty
			if (sourceLength == 0)
			{
				return targetLength;
			}
			if (targetLength == 0)
			{
				return sourceLength;
			}

			//Switch around variables so outer loop runs less
			if (targetLength < sourceLength)
			{
				var tempSource = source;
				source = target;
				target = tempSource;

				var tempSourceLength = sourceLength;
				sourceLength = targetLength;
				targetLength = tempSourceLength;
			}

			return Calculator.CalculateDistance(
				source.Slice(startIndex, sourceLength),
				target.Slice(startIndex, targetLength)
			);
		}
	}
}
