using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class Sse2Vector128StringCompareCalculator : ILevenshteinDistanceCalculator
	{
		public unsafe int CalculateDistance(string source, string target)
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

			var arrayPool = ArrayPool<ushort>.Shared;
			var rowComparison = arrayPool.Rent(target.Length);

			for (var i = 1; i <= source.Length; ++i)
			{
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
					}
					
					for (; targetIndex < target.Length; targetIndex++)
					{
						rowComparison[targetIndex] = sourceChar == target[targetIndex] ? ushort.MaxValue : ushort.MinValue;
					}
				}

				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i][j - 1] + 1;
					var delete = costMatrix[i - 1][j] + 1;
					var edit = costMatrix[i - 1][j - 1] + (~rowComparison[j - 1] & 1);

					costMatrix[i][j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			arrayPool.Return(rowComparison);

			return costMatrix[source.Length][target.Length];
		}
	}
}
