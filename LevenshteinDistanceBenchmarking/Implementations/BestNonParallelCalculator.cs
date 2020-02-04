using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	class BestNonParallelCalculator : ILevenshteinDistanceSpanCalculator
	{
		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
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
					var deleteCost = previousRow[j];

					if (sourcePrevChar == target[j - 1])
					{
						previousColumn = previousDiagonal;
					}
					else
					{
						var insertOrDelete = Math.Min(previousColumn, deleteCost);
						var edit = previousDiagonal;
						previousColumn = Math.Min(insertOrDelete, edit) + 1;
					}

					previousDiagonal = deleteCost;
					previousRow[j] = previousColumn;

					if (j == target.Length)
					{
						break;
					}

					deleteCost = previousRow[j + 1];

					if (sourcePrevChar == target[j])
					{
						previousColumn = previousDiagonal;
					}
					else
					{
						var insertOrDelete = Math.Min(previousColumn, deleteCost);
						var edit = previousDiagonal;
						previousColumn = Math.Min(insertOrDelete, edit) + 1;
					}

					previousDiagonal = deleteCost;
					previousRow[j + 1] = previousColumn;
				}
			}

			var result = previousRow[targetLength];
			arrayPool.Return(previousRow);
			return result;
		}
	}
}
