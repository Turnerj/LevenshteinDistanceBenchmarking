﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements.DataStructure
{
	class UnmanagedDenseMatrixCalculator : ILevenshteinDistanceSpanCalculator
	{
		public unsafe int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var rows = source.Length + 1;
			var columns = target.Length + 1;

			var costMatrixPointer = Marshal.AllocHGlobal(Marshal.SizeOf<int>() * columns * rows);
			var costMatrix = new Span<int>(costMatrixPointer.ToPointer(), rows * columns);

			costMatrix[0] = 0;

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i * columns] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i * columns + j - 1] + 1;
					var delete = costMatrix[(i - 1) * columns + j] + 1;
					var edit = costMatrix[(i - 1) * columns + j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

					costMatrix[i * columns + j] = Math.Min(Math.Min(insert, delete), edit);
				}
			}

			var result = costMatrix[(rows * columns) - 1];
			Marshal.FreeHGlobal(costMatrixPointer);
			return result;
		}
	}
}
