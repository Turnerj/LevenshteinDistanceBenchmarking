using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	public class BestNonParallelIntrinsicCalculator : ILevenshteinDistanceSpanCalculator
	{
		public unsafe int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;
			var columns = targetLength + 1;

			var vectorOffset = Vector256<ushort>.Count;
			var vectorOffsetTargetLength = targetLength - vectorOffset;

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
				var sourcePrevCharVector = Vector256.Create(sourcePrevChar);

				var columnIndex = 1;

				//Intrinsic Loop
				fixed (char* targetPtr = target)
				{
					var ushortTargetPtr = (ushort*)targetPtr;
					var targetIndex = 0;
					int insertOrDelete;
					int edit;

					for (; columnIndex <= vectorOffsetTargetLength; targetIndex += vectorOffset)
					{
						var targetCharVector = Avx.LoadVector256(ushortTargetPtr + targetIndex);
						var charEqualityVector = Avx2.CompareEqual(sourcePrevCharVector, targetCharVector);

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(0) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(1) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(2) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(3) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(4) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(5) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(6) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(7) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(8) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(9) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(10) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(11) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(12) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(13) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(14) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;

						insertOrDelete = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
						edit = previousDiagonal + (~charEqualityVector.GetElement(15) & 1);

						previousColumn = Math.Min(insertOrDelete, edit);
						previousDiagonal = previousRow[columnIndex];
						previousRow[columnIndex] = previousColumn;

						columnIndex++;
					}
				}

				//Non-intrinsic Loop
				for (; columnIndex <= targetLength; columnIndex += 2)
				{
					var insertOrDelete1 = Math.Min(previousColumn, previousRow[columnIndex]) + 1;
					var edit1 = previousDiagonal + (sourcePrevChar == target[columnIndex - 1] ? 0 : 1);

					previousColumn = Math.Min(insertOrDelete1, edit1);
					previousDiagonal = previousRow[columnIndex];
					previousRow[columnIndex] = previousColumn;

					if (columnIndex == target.Length)
					{
						break;
					}

					var insertOrDelete2 = Math.Min(previousColumn, previousRow[columnIndex + 1]) + 1;
					var edit2 = previousDiagonal + (sourcePrevChar == target[columnIndex] ? 0 : 1);

					previousColumn = Math.Min(insertOrDelete2, edit2);
					previousDiagonal = previousRow[columnIndex + 1];
					previousRow[columnIndex + 1] = previousColumn;
				}
			}

			var result = previousRow[targetLength];
			arrayPool.Return(previousRow);
			return result;
		}
	}
}
