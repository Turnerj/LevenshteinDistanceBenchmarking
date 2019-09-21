using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Comparisons
{
	class BestNonParallelIntrinsicCalculatorComparison : ILevenshteinDistanceSpanCalculator
	{
		public unsafe int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;
			var columns = targetLength + 1;

			var rentedPool = ArrayPool<int>.Shared.Rent(2 * columns);
			var costMatrix = new Span<int>(rentedPool);

			for (var i = 1; i <= targetLength; ++i)
			{
				costMatrix[i] = i;
			}

			costMatrix[0] = 0;
			var allOnesVectors = Vector256.Create(1);

			var rowComparison = ArrayPool<ushort>.Shared.Rent(target.Length);

			for (var i = 1; i <= sourceLength; ++i)
			{
				var currentRow = costMatrix.Slice((i & 1) * columns);
				currentRow[0] = i;

				var previousRow = costMatrix.Slice(((i - 1) & 1) * columns);

				fixed (int* prevRowPtr = previousRow)
				fixed (ushort* rowComparisonPtr = rowComparison)
				fixed (char* targetPtr = target)
				{
					var ushortTargetPtr = (ushort*)targetPtr;

					var sourceChar = (ushort)source[i - 1];
					var sourceCharVector = Vector128.Create(sourceChar);

					var targetIndex = 0;

					for (; targetIndex + Vector128<ushort>.Count < target.Length; targetIndex += Vector128<ushort>.Count)
					{
						var targetVector = Sse2.LoadVector128(ushortTargetPtr + targetIndex);
						var vectorCompare = Sse2.CompareEqual(sourceCharVector, targetVector);
						Sse2.Store(rowComparisonPtr + targetIndex, vectorCompare);

						//Vector256<int>.Count == Vector128<ushort>.Count: This is why this can be in the same loop
						var columnsCovered = Avx.LoadVector256(prevRowPtr + targetIndex);
						var addedColumns = Avx2.Add(columnsCovered, allOnesVectors);
						Avx.Store(prevRowPtr + targetIndex, addedColumns);
					}

					for (; targetIndex < target.Length; targetIndex++)
					{
						rowComparison[targetIndex] = sourceChar == target[targetIndex] ? ushort.MaxValue : ushort.MinValue;
						previousRow[targetIndex]++;
					}

					previousRow[targetIndex + 1]++;
				}

				for (var j = 1; j <= targetLength; ++j)
				{
					var insert = currentRow[j - 1] + 1;
					var delete = previousRow[j];
					var edit = previousRow[j - 1] - (rowComparison[j - 1] & 1);

					currentRow[j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			var result = costMatrix[(sourceLength & 1) * columns + targetLength];
			ArrayPool<int>.Shared.Return(rentedPool);
			ArrayPool<ushort>.Shared.Return(rowComparison);
			return result;
		}
	}
}
