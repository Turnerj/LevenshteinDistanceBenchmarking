﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements.Intrinsic
{
	class VectorAdditionCalculator : ILevenshteinDistanceSpanCalculator
	{
		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var columns = target.Length + 1;
			columns += Vector<int>.Count - (columns & (Vector<int>.Count - 1));

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

			for (var i = 1; i <= source.Length; ++i)
			{
				var previousRow = new Span<int>(costMatrix[i - 1]);
				var vectorRowSpan = MemoryMarshal.Cast<int, Vector<int>>(previousRow);
				for (var vectorIndex = 0; vectorIndex < vectorRowSpan.Length; vectorIndex++)
				{
					var result = vectorRowSpan[vectorIndex] + Vector<int>.One;
					vectorRowSpan[vectorIndex] = result;
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
