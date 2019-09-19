using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class BestNonParallelIntrinsicCalculator : ILevenshteinDistanceCalculator
	{
		public unsafe int CalculateDistance(string source, string target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;
			var columns = targetLength + 1;
			
			//Align to nearest Vector256
			columns += Vector256<int>.Count - (columns & (Vector256<int>.Count - 1));

			var arrayPool = ArrayPool<int>.Shared;
			var rentedPool = arrayPool.Rent(2 * columns);
			var costMatrix = new Span<int>(rentedPool);

			for (var i = 1; i <= targetLength; ++i)
			{
				costMatrix[i] = i;
			}

			costMatrix[0] = 0;
			var allOnesVectors = Vector256.Create(1);

			for (var i = 1; i <= sourceLength; ++i)
			{
				var currentRow = costMatrix.Slice((i & 1) * columns);
				currentRow[0] = i;

				var previousRow = costMatrix.Slice(((i - 1) & 1) * columns);

				fixed (int* prevRowPtr = previousRow)
				{
					for (int columnIndex = 0, l = target.Length + 1; columnIndex <= l; columnIndex += Vector256<int>.Count)
					{
						var columnsCovered = Avx.LoadVector256(prevRowPtr + columnIndex);
						var addedColumns = Avx2.Add(columnsCovered, allOnesVectors);
						Avx.Store(prevRowPtr + columnIndex, addedColumns);
					}
				}

				var sourcePrevChar = source[i - 1];

				for (var j = 1; j <= targetLength; ++j)
				{
					var insert = currentRow[j - 1] + 1;
					var delete = previousRow[j];
					var edit = previousRow[j - 1];

					if (sourcePrevChar == target[j - 1])
					{
						edit--;
					}

					currentRow[j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			var result = costMatrix[(sourceLength & 1) * columns + targetLength];
			arrayPool.Return(rentedPool);
			return result;
		}
	}
}
