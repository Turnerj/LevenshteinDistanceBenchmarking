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
			var rentedPool = arrayPool.Rent(2 * columns);
			var costMatrix = new Span<int>(rentedPool);

			for (var i = 1; i <= targetLength; ++i)
			{
				costMatrix[i] = i;
			}

			costMatrix[0] = 0;

			for (var i = 1; i <= sourceLength; ++i)
			{
				var currentRow = costMatrix.Slice((i & 1) * columns);
				currentRow[0] = i;

				var previousRow = costMatrix.Slice(((i - 1) & 1) * columns);
				var sourcePrevChar = source[i - 1];

				for (var j = 1; j <= targetLength; j += 2)
				{
					var insert1 = currentRow[j - 1] + 1;
					var delete1 = previousRow[j] + 1;
					var edit1 = previousRow[j - 1] + (sourcePrevChar == target[j - 1] ? 0 : 1);

					var result1 = Math.Min(Math.Min(insert1, delete1), edit1);
					currentRow[j] = result1;

					if (j == target.Length)
					{
						break;
					}

					var insert2 = result1 + 1;
					var delete2 = previousRow[j + 1] + 1;
					var edit2 = previousRow[j] + (sourcePrevChar == target[j] ? 0 : 1);

					var result2 = Math.Min(Math.Min(insert2, delete2), edit2);
					currentRow[j + 1] = result2;
				}
			}

			var result = costMatrix[(sourceLength & 1) * columns + targetLength];
			arrayPool.Return(rentedPool);
			return result;
		}
	}
}
