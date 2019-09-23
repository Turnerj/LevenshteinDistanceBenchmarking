using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements.DataStructure
{
	class MultiDimensionMatrixCalculator : ILevenshteinDistanceSpanCalculator
	{
		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var costMatrix = new int[source.Length + 1, target.Length + 1];

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i, 0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0, i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i, j - 1] + 1;
					var delete = costMatrix[i - 1, j] + 1;
					var edit = costMatrix[i - 1, j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					costMatrix[i, j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			return costMatrix[source.Length, target.Length];
		}
	}
}
