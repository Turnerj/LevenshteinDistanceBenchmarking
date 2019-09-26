using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Frontends
{
	public class MultiLineSubsectionFrontend : ILevenshteinDistanceSpanCalculator
	{
		private readonly ILevenshteinDistanceSpanCalculator Calculator = new LevenshteinDistanceBaseline();

		private enum EditOperationKind : byte
		{
			None,
			Add,
			Edit,
			Remove
		}

		private class EditOperation
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

		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var sourceLines = GetSubsectionHashes(source);
			var targetLines = GetSubsectionHashes(target);

			var subsectionPairs = GetSubsectionsPairs(sourceLines, targetLines);
			var joinedPairs = subsectionPairs;//JoinSubsectionsOnProximity(subsectionPairs, 32);
			var talliedDistance = 0;

			for (var i = 0; i < joinedPairs.Length; i++)
			{
				var subsectionPair = joinedPairs[i];
				var sourceLine = source.Slice(subsectionPair.Source.Index, subsectionPair.Source.Length);
				Debug.Write("Source: "); Debug.WriteLine(sourceLine.ToString());
				var targetLine = target.Slice(subsectionPair.Target.Index, subsectionPair.Target.Length);
				Debug.Write("Target: "); Debug.WriteLine(targetLine.ToString());
				talliedDistance += Calculator.CalculateDistance(sourceLine, targetLine);
			}

			return talliedDistance;
		}

		private SubsectionPair[] JoinSubsectionsOnProximity(SubsectionPair[] subsections, int proximity)
		{
			var joinedSubsections = new List<SubsectionPair>();

			var sourceStartIndex = -1;
			var talliedSourceLength = 0;
			var targetStartIndex = -1;
			var talliedTargetLength = 0;

			for (var i = 0; i < subsections.Length; i++)
			{
				var subsection = subsections[i];
				if (sourceStartIndex == -1)
				{
					sourceStartIndex = subsection.Source.Index;
					talliedSourceLength = subsection.Source.Length;
					targetStartIndex = subsection.Target.Index;
					talliedTargetLength = subsection.Target.Length;
					continue;
				}

				var sourceEndIndex = sourceStartIndex + talliedSourceLength;
				var targetEndIndex = targetStartIndex + talliedTargetLength;

				if (
					sourceEndIndex >= subsection.Source.Index - proximity ||
					targetEndIndex >= subsection.Target.Index - proximity
				)
				{
					talliedSourceLength += subsection.Source.Index - sourceEndIndex + subsection.Source.Length;
					talliedTargetLength += subsection.Target.Index - targetEndIndex + subsection.Target.Length;
				}
				else
				{
					joinedSubsections.Add(new SubsectionPair(
						new SubsectionInfo(null, sourceStartIndex, talliedSourceLength),
						new SubsectionInfo(null, targetStartIndex, talliedTargetLength),
						null
					));
					sourceStartIndex = -1;
				}
			}

			if (sourceStartIndex != -1)
			{
				joinedSubsections.Add(new SubsectionPair(
					new SubsectionInfo(null, sourceStartIndex, talliedSourceLength),
					new SubsectionInfo(null, targetStartIndex, talliedTargetLength),
					null
				));
			}

			return joinedSubsections.ToArray();
		}

		private SubsectionPair[] GetSubsectionsPairs(SubsectionInfo[] sourceLines, SubsectionInfo[] targetLines)
		{
			var opMatrix = Enumerable
				.Range(0, sourceLines.Length + 1)
				.Select(line => new EditOperationKind[targetLines.Length + 1])
				.ToArray();

			var costMatrix = Enumerable
				.Range(0, 2)
				.Select(line => new int[targetLines.Length + 1])
				.ToArray();

			for (var i = 1; i <= targetLines.Length; ++i)
			{
				opMatrix[0][i] = EditOperationKind.Add;
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= sourceLines.Length; ++i)
			{
				costMatrix[i & 1][0] = i;
				opMatrix[i][0] = EditOperationKind.Remove;

				for (var j = 1; j <= targetLines.Length; ++j)
				{
					var insert = costMatrix[i & 1][j - 1] + 1;
					var delete = costMatrix[(i - 1) & 1][j] + 1;
					var edit = costMatrix[(i - 1) & 1][j - 1] + (sourceLines[i - 1].Hash == targetLines[j - 1].Hash ? 0 : 1);

					var min = Math.Min(Math.Min(insert, delete), edit);

					if (min == insert)
					{
						opMatrix[i][j] = EditOperationKind.Add;
					}
					else if (min == delete)
					{
						opMatrix[i][j] = EditOperationKind.Remove;
					}
					else if (min == edit)
					{
						opMatrix[i][j] = EditOperationKind.Edit;
					}

					costMatrix[i & 1][j] = min;
				}
			}

			var trackedOperations = new Stack<EditOperation>(sourceLines.Length + targetLines.Length);

			for (int x = targetLines.Length, y = sourceLines.Length; (x > 0) || (y > 0);)
			{
				var op = opMatrix[y][x];

				if (op == EditOperationKind.Add)
				{
					x -= 1;
					trackedOperations.Push(new EditOperation(default, targetLines[x], op));
				}
				else if (op == EditOperationKind.Remove)
				{
					y -= 1;
					trackedOperations.Push(new EditOperation(sourceLines[y], default, op));
				}
				else if (op == EditOperationKind.Edit)
				{
					x -= 1;
					y -= 1;

					if (sourceLines[y].Hash == targetLines[x].Hash)
					{
						op = EditOperationKind.None;
					}

					trackedOperations.Push(new EditOperation(sourceLines[y], targetLines[x], op));
				}
				else // Start of the matching (EditOperationKind.None)
					break;
			}

			var mergedOps = MergeOperations(trackedOperations.ToArray());
			return BuildSubsectionRange(mergedOps.ToArray());
		}

		private EditOperation[] MergeOperations(EditOperation[] editOperations)
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

				//The problem here is that I use "default" for different types of edit operations
				//When I loop over the operation data, I actually need to track not only the first/last indexes but 
				//the source/target indexes individually too (plus lengths)

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
