﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class Avx2Vector256AdditionCalculator : ILevenshteinDistanceSpanCalculator
	{
		public unsafe int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var columns = target.Length + 1;
			columns += Vector256<int>.Count - (columns & (Vector256<int>.Count - 1));

			var costMatrix = Enumerable
			  .Range(0, source.Length + 1)
			  .Select(line => new int[columns])
			  .ToArray();

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0][i] = i;
			}

			var allOnesVectors = Vector256.Create(1);

			for (var i = 1; i <= source.Length; ++i)
			{
				fixed (int* prevRowPtr = costMatrix[i - 1])
				{
					var previousRow = new Span<int>(costMatrix[i - 1]);
					for (int columnIndex = 0, l = target.Length + 1; columnIndex <= l; columnIndex += Vector256<int>.Count)
					{
						var columnsCovered = Avx.LoadVector256(prevRowPtr + columnIndex);
						var addedColumns = Avx2.Add(columnsCovered, allOnesVectors);
						Avx.Store(prevRowPtr + columnIndex, addedColumns);
					}
				}

				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i][j - 1] + 1;
					var delete = costMatrix[i - 1][j];
					var edit = costMatrix[i - 1][j - 1];

					if (source[i - 1] == target[j - 1])
					{
						edit -= 1;
					}

					costMatrix[i][j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
