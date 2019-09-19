using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class DoubleOpCalculator : ILevenshteinDistanceCalculator
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
				for (var j = 1; j <= target.Length; j += 2)
				{
					var insertA = costMatrix[i][j - 1] + 1;
					var deleteA = costMatrix[i - 1][j] + 1;
					var editA = costMatrix[i - 1][j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					var resultA = Math.Min(Math.Min(insertA, deleteA), editA);
					costMatrix[i][j] = resultA;

					if (j == target.Length)
					{
						break;
					}

					var insertB = resultA + 1;
					var deleteB = costMatrix[i - 1][j + 1] + 1;
					var editB = costMatrix[i - 1][j] + (source[i - 1] == target[j] ? 0 : 1);

					var resultB = Math.Min(Math.Min(insertB, deleteB), editB);
					costMatrix[i][j + 1] = resultB;
				}
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
