using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class BestNonParallelCalculator : ILevenshteinDistanceCalculator
	{
		public int CalculateDistance(string source, string target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;
			var columns = targetLength + 1;

			var arrayPool = ArrayPool<int>.Shared;
			var previousRow = arrayPool.Rent(columns);

			for (var i = 1; i <= targetLength; ++i)
			{
				previousRow[i] = i;
			}

			previousRow[0] = 0;

			for (var i = 1; i <= sourceLength; ++i)
			{
				var previousDiagonal = previousRow[0];
				var previousColumn = previousRow[0]++;

				var sourcePrevChar = source[i - 1];

				for (var j = 1; j <= targetLength; j += 2)
				{
					var insertOrDelete1 = Math.Min(previousColumn, previousRow[j]) + 1;
					var edit1 = previousDiagonal + (sourcePrevChar == target[j - 1] ? 0 : 1);

					previousColumn = Math.Min(insertOrDelete1, edit1);
					previousDiagonal = previousRow[j];
					previousRow[j] = previousColumn;

					if (j == target.Length)
					{
						break;
					}

					var insertOrDelete2 = Math.Min(previousColumn, previousRow[j + 1]) + 1;
					var edit2 = previousDiagonal + (sourcePrevChar == target[j] ? 0 : 1);

					previousColumn = Math.Min(insertOrDelete2, edit2);
					previousDiagonal = previousRow[j + 1];
					previousRow[j + 1] = previousColumn;
				}
			}

			var result = previousRow[targetLength];
			arrayPool.Return(previousRow);
			return result;
		}
	}
}
