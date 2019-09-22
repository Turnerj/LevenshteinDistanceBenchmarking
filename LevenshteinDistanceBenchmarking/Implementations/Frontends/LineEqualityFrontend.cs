using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Frontends
{
	public class LineEqualityFrontend : ILevenshteinDistanceSpanCalculator
	{
		private readonly ILevenshteinDistanceSpanCalculator Calculator = new LevenshteinDistanceBaseline();

		private enum EditOperationKind : byte
		{
			None,
			Add,
			Edit,
			Remove
		}

		private struct DiffSubsection
		{
			public readonly int SourceIndex;
			public readonly int SourceLength;
			public readonly int TargetIndex;
			public readonly int TargetLength;

			public DiffSubsection(int sourceIndex, int sourceLength, int targetIndex, int targetLength)
			{
				SourceIndex = sourceIndex;
				SourceLength = sourceLength;
				TargetIndex = targetIndex;
				TargetLength = targetLength;
			}
		}

		private class LineInfo
		{
			public readonly string Hash;
			public readonly int Index;
			public readonly int Length;

			public static readonly LineInfo NoLine = new LineInfo();

			private LineInfo() { }
			public LineInfo(string hash, int index, int length)
			{
				Hash = hash;
				Index = index;
				Length = length;
			}
		}

		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var sourceLines = GetLineHashes(source);
			var targetLines = GetLineHashes(target);

			var diffSubsections = GetDiffSubsections(sourceLines, targetLines);
			var talliedDistance = 0;

			for (var i = 0; i < diffSubsections.Length; i++)
			{
				var diffSubsection = diffSubsections[i];
				var sourceLine = source.Slice(diffSubsection.SourceIndex, diffSubsection.SourceLength);
				var targetLine = target.Slice(diffSubsection.TargetIndex, diffSubsection.TargetLength);
				talliedDistance += Calculator.CalculateDistance(sourceLine, targetLine);
			}

			return talliedDistance;
		}

		private DiffSubsection[] GetDiffSubsections(List<LineInfo> sourceLines, List<LineInfo> targetLines)
		{
			var opMatrix = Enumerable
				.Range(0, sourceLines.Count + 1)
				.Select(line => new EditOperationKind[targetLines.Count + 1])
				.ToArray();

			var costMatrix = Enumerable
				.Range(0, 2)
				.Select(line => new int[targetLines.Count + 1])
				.ToArray();

			for (var i = 1; i <= targetLines.Count; ++i)
			{
				opMatrix[0][i] = EditOperationKind.Add;
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= sourceLines.Count; ++i)
			{
				costMatrix[i & 1][0] = i;
				opMatrix[i][0] = EditOperationKind.Remove;

				for (var j = 1; j <= targetLines.Count; ++j)
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

			var result = new Stack<DiffSubsection>(Math.Max(sourceLines.Count, targetLines.Count));

			var xLength = 0;
			var yLength = 0;

			for (int x = targetLines.Count, y = sourceLines.Count; (x > 0) || (y > 0);)
			{
				var op = opMatrix[y][x];

				if (op == EditOperationKind.Add)
				{
					x -= 1;
					xLength += targetLines[x].Length;
				}
				else if (op == EditOperationKind.Remove)
				{
					y -= 1;
					yLength += sourceLines[y].Length;
				}
				else if (op == EditOperationKind.Edit)
				{
					x -= 1;
					y -= 1;

					xLength += targetLines[x].Length;
					yLength += sourceLines[y].Length;

					var sourceLine = sourceLines[y];
					var targetLine = targetLines[x];
					if (sourceLine.Hash == targetLine.Hash)
					{
						result.Push(new DiffSubsection(sourceLine.Index, yLength, targetLine.Index, xLength));
						xLength = 0;
						yLength = 0;
					}
				}
				else // Start of the matching (EditOperationKind.None)
					break;
			}

			result.Push(new DiffSubsection(sourceLines[0].Index, yLength, targetLines[0].Index, xLength));

			return result.ToArray();
		}

		private List<LineInfo> GetLineHashes(ReadOnlySpan<char> text)
		{
			var result = new List<LineInfo>();
			var lineStart = 0;
			var cursor = 0;

			static LineInfo CaptureLineInfo(ReadOnlySpan<char> text, int lineStart, int cursor)
			{
				var length = cursor - lineStart;
				var line = text.Slice(lineStart, length);
				var hash = CreateMD5(line);
				return new LineInfo(hash, lineStart, length);
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

			return result;
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
