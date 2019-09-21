using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class LocalRowDataCalculator : ILevenshteinDistanceSpanCalculator
	{
		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var targetLength = target.Length;
			var costMatrix = Enumerable
			  .Range(0, source.Length + 1)
			  .Select(line => new int[targetLength + 1])
			  .ToArray();

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				var currentRow = costMatrix[i];
				var previousRow = costMatrix[i - 1];

				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = currentRow[j - 1] + 1;
					var delete = previousRow[j] + 1;
					var edit = previousRow[j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					currentRow[j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
