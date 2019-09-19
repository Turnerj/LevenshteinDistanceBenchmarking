using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class SingleLoopArrayPoolDenseMatrixCalculator : ILevenshteinDistanceCalculator
	{
		public int CalculateDistance(string source, string target)
		{
			var rows = source.Length + 1;
			var columns = target.Length + 1;

			var arrayPool = ArrayPool<int>.Shared;
			var costMatrix = arrayPool.Rent(rows * columns);

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i * columns] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[i] = i;
			}

			costMatrix[0] = 0;

			//Note: These short circuits are necessary
			if (target.Length == 0)
			{
				return source.Length;
			}
			if (source.Length == 0)
			{
				return target.Length;
			}

			var totalSize = rows * columns;

			var columnTrack = 1;

			for (var i = columns + 1; i < totalSize; ++i, columnTrack++)
			{
				if (columnTrack == columns)
				{
					i++;
					columnTrack = 1;
				}

				var insert = costMatrix[i - 1] + 1;
				var delete = costMatrix[i - columns] + 1;
				var edit = costMatrix[i - columns - 1];

				var sourceIndex = i / columns;
				var targetIndex = i - sourceIndex * columns;
				if (source[sourceIndex - 1] != target[targetIndex - 1])
				{
					edit += 1;
				}

				costMatrix[i] = Math.Min(Math.Min(insert, delete), edit);
			}

			var result = costMatrix[(rows * columns) - 1];
			arrayPool.Return(costMatrix);
			return result;
		}
	}
}
