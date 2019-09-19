using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class LocalLastInsertCalculator : ILevenshteinDistanceCalculator
	{
		public int CalculateDistance(string source, string target)
		{
			var costMatrix = Enumerable
			  .Range(0, source.Length + 1)
			  .Select(line => new int[target.Length + 1])
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
				var lastInsert = costMatrix[i][0];
				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = lastInsert + 1;
					var delete = costMatrix[i - 1][j] + 1;
					var edit = costMatrix[i - 1][j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					lastInsert = Math.Min(Math.Min(insert, delete), edit);
					costMatrix[i][j] = lastInsert;
				}
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
