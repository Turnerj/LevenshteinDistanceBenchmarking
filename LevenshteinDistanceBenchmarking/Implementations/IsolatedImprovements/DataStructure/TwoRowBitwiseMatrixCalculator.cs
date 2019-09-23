using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements.DataStructure
{
	class TwoRowBitwiseMatrixCalculator : ILevenshteinDistanceSpanCalculator
	{
		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var targetLength = target.Length;
			var costMatrix = Enumerable
			  .Range(0, 2)
			  .Select(line => new int[targetLength + 1])
			  .ToArray();

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i & 1][0] = i;

				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i & 1][j - 1] + 1;
					var delete = costMatrix[(i - 1) & 1][j] + 1;
					var edit = costMatrix[(i - 1) & 1][j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					costMatrix[i & 1][j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			return costMatrix[source.Length & 1][target.Length];
		}
	}
}
