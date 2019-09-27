using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LevenshteinDistanceBenchmarking.Implementations.Frontends.Comparisons
{
	public class MultiLineSubsectionFrontendComparison : ILevenshteinDistanceMemoryCalculator
	{
		private readonly ILevenshteinDistanceSpanCalculator Calculator = new LevenshteinDistanceBaseline();

		private enum EditOperationKind : byte
		{
			None,
			Add,
			Edit,
			Remove
		}

		private struct EditOperation
		{
			public readonly SubsectionInfo Source;
			public readonly SubsectionInfo Target;
			public readonly EditOperationKind OperationKind;

			public EditOperation(SubsectionInfo source, SubsectionInfo target, EditOperationKind operationKind)
			{
				Source = source;
				Target = target;
				OperationKind = operationKind;
			}
		}

		private struct SubsectionPair
		{
			public readonly SubsectionInfo Source;
			public readonly SubsectionInfo Target;
			public readonly EditOperationKind[] Ops;

			public SubsectionPair(SubsectionInfo source, SubsectionInfo target, EditOperationKind[] ops)
			{
				Source = source;
				Target = target;
				Ops = ops;
			}
		}

		private struct SubsectionInfo
		{
			public readonly string Hash;
			public readonly int Index;
			public readonly int Length;

			public SubsectionInfo(string hash, int index, int length)
			{
				Hash = hash;
				Index = index;
				Length = length;
			}
		}

		public int CalculateDistance(ReadOnlyMemory<char> source, ReadOnlyMemory<char> target)
		{
			var sourceSpan = source.Span;
			var targetSpan = target.Span;

			var maxSize = Math.Max(source.Length, target.Length);
			
			if (maxSize > 512)
			{
				var sourceLines = GetSubsectionHashes(sourceSpan);
				var targetLines = GetSubsectionHashes(targetSpan);

				var subsectionPairs = GetSubsectionsPairs(sourceLines, targetLines);
				var talliedDistance = 0;

				if (subsectionPairs.Length > 1)
				{
					Parallel.For(0, subsectionPairs.Length, subsectionIndex =>
					{
						var localSourceSpan = source.Span;
						var localTargetSpan = target.Span;
						var subsectionPair = subsectionPairs[subsectionIndex];
						var sourceLine = localSourceSpan.Slice(subsectionPair.Source.Index, subsectionPair.Source.Length);
						var targetLine = localTargetSpan.Slice(subsectionPair.Target.Index, subsectionPair.Target.Length);
						var subsectionDistance = Calculator.CalculateDistance(sourceLine, targetLine);
						Interlocked.Add(ref talliedDistance, subsectionDistance);
					});
				}
				else
				{
					talliedDistance = Calculator.CalculateDistance(sourceSpan, targetSpan);
				}

				return talliedDistance;
			}
			else
			{
				return Calculator.CalculateDistance(sourceSpan, targetSpan);
			}
		}

		private SubsectionPair[] GetSubsectionsPairs(SubsectionInfo[] sourceLines, SubsectionInfo[] targetLines)
		{
			var rows = sourceLines.Length + 1;
			var columns = targetLines.Length + 1;

			var opMatrix = ArrayPool<EditOperationKind>.Shared.Rent(rows * columns);
			var opSpan = new Span<EditOperationKind>(opMatrix);
			var previousRow = ArrayPool<int>.Shared.Rent(targetLines.Length + 1);

			for (var i = 1; i <= targetLines.Length; ++i)
			{
				opMatrix[i] = EditOperationKind.Add;
				previousRow[i] = i;
			}

			previousRow[0] = 0;

			for (var i = 1; i <= sourceLines.Length; ++i)
			{
				var opRow = opSpan.Slice(i * columns, columns);
				opRow[0] = EditOperationKind.Remove;

				var previousDiagonal = previousRow[0];
				var previousColumn = previousRow[0]++;

				for (var j = 1; j <= targetLines.Length; ++j)
				{
					var insert = previousColumn + 1;
					var delete = previousRow[j] + 1;
					var edit = previousDiagonal + (sourceLines[i - 1].Hash == targetLines[j - 1].Hash ? 0 : 1);

					previousColumn = Math.Min(Math.Min(insert, delete), edit);

					if (previousColumn == insert)
					{
						opRow[j] = EditOperationKind.Add;
					}
					else if (previousColumn == delete)
					{
						opRow[j] = EditOperationKind.Remove;
					}
					else if (previousColumn == edit)
					{
						opRow[j] = EditOperationKind.Edit;
					}

					previousDiagonal = previousRow[j];
					previousRow[j] = previousColumn;
				}
			}

			ArrayPool<int>.Shared.Return(previousRow);

			var trackedOperations = new Span<EditOperation>(new EditOperation[sourceLines.Length + targetLines.Length]);
			var operationIndex = trackedOperations.Length - 1;

			for (int x = targetLines.Length, y = sourceLines.Length; (x > 0) || (y > 0); operationIndex--)
			{
				var op = opMatrix[y * columns + x];

				if (op == EditOperationKind.Add)
				{
					x -= 1;
					trackedOperations[operationIndex] = new EditOperation(default, targetLines[x], op);
				}
				else if (op == EditOperationKind.Remove)
				{
					y -= 1;
					trackedOperations[operationIndex] = new EditOperation(sourceLines[y], default, op);
				}
				else if (op == EditOperationKind.Edit)
				{
					x -= 1;
					y -= 1;

					if (sourceLines[y].Hash == targetLines[x].Hash)
					{
						op = EditOperationKind.None;
					}

					trackedOperations[operationIndex] = new EditOperation(sourceLines[y], targetLines[x], op);
				}
				else // Start of the matching (EditOperationKind.None)
					break;
			}

			ArrayPool<EditOperationKind>.Shared.Return(opMatrix);

			var mergedOps = MergeOperations(trackedOperations.Slice(operationIndex + 1));
			return BuildSubsectionRange(mergedOps.ToArray());
		}

		private EditOperation[] MergeOperations(ReadOnlySpan<EditOperation> editOperations)
		{
			var result = new List<EditOperation>();

			for (int i = 0, l = editOperations.Length; i < l; i++)
			{
				var editOperation = editOperations[i];
				if (editOperation.OperationKind == EditOperationKind.None)
				{
					result.Add(editOperation);
				}
				else
				{
					var sourceIndex = -1;
					var sourceLength = 0;
					var targetIndex = -1;
					var targetLength = 0;
					for (; i < l; i++)
					{
						editOperation = editOperations[i];
						if (editOperation.OperationKind == EditOperationKind.None)
						{
							break;
						}

						if (editOperation.OperationKind == EditOperationKind.Add || editOperation.OperationKind == EditOperationKind.Edit)
						{
							if (targetIndex == -1)
							{
								targetIndex = editOperation.Target.Index;
							}
							targetLength += editOperation.Target.Length;
						}

						if (editOperation.OperationKind == EditOperationKind.Remove || editOperation.OperationKind == EditOperationKind.Edit)
						{
							if (sourceIndex == -1)
							{
								sourceIndex = editOperation.Source.Index;
							}
							sourceLength += editOperation.Source.Length;
						}
					}

					if (sourceIndex == -1)
					{
						sourceIndex = 0;
					}
					if (targetIndex == -1)
					{
						targetIndex = 0;
					}

					result.Add(
						new EditOperation(
							new SubsectionInfo(null, sourceIndex, sourceLength), 
							new SubsectionInfo(null, targetIndex, targetLength),
							EditOperationKind.Edit
						)
					);

					if (editOperation.OperationKind == EditOperationKind.None)
					{
						result.Add(editOperation);
					}
				}
			}

			return result.ToArray();
		}

		private SubsectionPair[] BuildSubsectionRange(EditOperation[] editOperations)
		{
			var firstMatchIndex = -1;
			var lastMatchIndex = -1;
			var noChangeGap = 0;
			var maxGap = 3;

			SubsectionPair CreateSubsection(int operationStartIndex, int operationEndIndex)
			{
				var startBoundaryIndex = operationStartIndex;
				var endBoundaryIndex = operationEndIndex;
				for (var i = 0; startBoundaryIndex > 0 && i < maxGap; i++, startBoundaryIndex--) ;
				for (var i = 0; endBoundaryIndex < editOperations.Length - 1 && i < maxGap; i++, endBoundaryIndex++) ;

				var firstOperation = editOperations[startBoundaryIndex];
				var lastOperation = editOperations[endBoundaryIndex];

				var sourceIndex = firstOperation.Source.Index;
				var sourceLength = lastOperation.Source.Index - sourceIndex + lastOperation.Source.Length;
				var targetIndex = firstOperation.Target.Index;
				var targetLength = lastOperation.Target.Index - targetIndex + lastOperation.Target.Length;

				return new SubsectionPair(
					new SubsectionInfo(null, sourceIndex, sourceLength),
					new SubsectionInfo(null, targetIndex, targetLength),
					null
				);
			}

			var result = new List<SubsectionPair>(editOperations.Length / 2 + 1);

			for (int i = 0, l = editOperations.Length; i < l; i++)
			{
				var editOperation = editOperations[i];

				if (editOperation.OperationKind != EditOperationKind.None)
				{
					noChangeGap = 0;
					
					if (firstMatchIndex == -1)
					{
						firstMatchIndex = i;
						lastMatchIndex = i;
					}
					else
					{
						lastMatchIndex = i;
					}
				}
				else if (firstMatchIndex != -1)
				{
					noChangeGap++;

					if (noChangeGap == maxGap)
					{
						result.Add(CreateSubsection(firstMatchIndex, lastMatchIndex));
						firstMatchIndex = -1;
						lastMatchIndex = -1;
					}
				}
			}

			if (firstMatchIndex != -1)
			{
				result.Add(CreateSubsection(firstMatchIndex, lastMatchIndex));
			}

			return result.ToArray();
		}

		private SubsectionInfo[] GetSubsectionHashes(ReadOnlySpan<char> text)
		{
			var result = new List<SubsectionInfo>();
			var lineStart = 0;
			var cursor = 0;

			static SubsectionInfo CaptureLineInfo(ReadOnlySpan<char> text, int lineStart, int cursor)
			{
				var length = cursor - lineStart;
				var line = text.Slice(lineStart, length);
				var hash = CreateMD5(line);
				return new SubsectionInfo(hash, lineStart, length);
			}

			for (; cursor < text.Length; cursor++)
			{
				if (text[cursor] == '\n')
				{
					result.Add(
						CaptureLineInfo(text, lineStart, cursor + 1)
					);
					lineStart = cursor + 1;
				}
			}
			
			result.Add(
				CaptureLineInfo(text, lineStart, cursor)
			);

			return result.ToArray();
		}

		//Based on: https://stackoverflow.com/a/55490468/1676444
		private static string CreateMD5(ReadOnlySpan<char> text)
		{
			using (MD5 md5 = MD5.Create())
			{
				var encoding = Encoding.UTF8;
				var bytesRequired = encoding.GetByteCount(text);
				Span<byte> data = stackalloc byte[bytesRequired];
				encoding.GetBytes(text, data);

				Span<byte> hashBytes = stackalloc byte[16];
				md5.TryComputeHash(data, hashBytes, out int written);
				if (written != hashBytes.Length)
					throw new OverflowException();

				Span<char> stringBuffer = stackalloc char[32];
				for (int i = 0; i < hashBytes.Length; i++)
				{
					hashBytes[i].TryFormat(stringBuffer.Slice(2 * i), out _, "x2");
				}
				return new string(stringBuffer);
			}
		}
	}
}
