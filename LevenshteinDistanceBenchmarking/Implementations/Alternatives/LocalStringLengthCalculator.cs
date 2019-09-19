using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class LocalStringLengthCalculator : ILevenshteinDistanceCalculator
	{
		public int CalculateDistance(string source, string target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;

			var costMatrix = Enumerable
			  .Range(0, sourceLength + 1)
			  .Select(line => new int[targetLength + 1])
			  .ToArray();

			for (var i = 1; i <= sourceLength; ++i)
			{
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= targetLength; ++i)
			{
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= sourceLength; ++i)
			{
				for (var j = 1; j <= targetLength; ++j)
				{
					var insert = costMatrix[i][j - 1] + 1;
					var delete = costMatrix[i - 1][j] + 1;
					var edit = costMatrix[i - 1][j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					costMatrix[i][j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			return costMatrix[sourceLength][targetLength];
		}
	}
}
