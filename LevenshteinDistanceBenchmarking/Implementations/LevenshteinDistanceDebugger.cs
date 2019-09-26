using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations
{
	public class LevenshteinDistanceDebugger : ILevenshteinDistanceSpanCalculator
	{
		private enum EditOperationKind : byte
		{
			None,
			Add,
			Edit,
			Remove
		}
		private struct EditOperation
		{
			public EditOperation(char valueFrom, char valueTo, EditOperationKind operation)
			{
				ValueFrom = valueFrom;
				ValueTo = valueTo;

				Operation = valueFrom == valueTo ? EditOperationKind.None : operation;
			}

			public char ValueFrom { get; }
			public char ValueTo { get; }
			public EditOperationKind Operation { get; }

			public override string ToString()
			{
				switch (Operation)
				{
					case EditOperationKind.None:
						return $"'{ValueTo}' Equal";
					case EditOperationKind.Add:
						return $"'{ValueTo}' Add";
					case EditOperationKind.Remove:
						return $"'{ValueFrom}' Remove";
					case EditOperationKind.Edit:
						return $"'{ValueFrom}' to '{ValueTo}' Edit";
					default:
						return "???";
				}
			}
		}

		public int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;

			var opMatrix = Enumerable
				.Range(0, sourceLength + 1)
				.Select(line => new EditOperationKind[targetLength + 1])
				.ToArray();

			var costMatrix = Enumerable
				.Range(0, source.Length + 1)
				.Select(line => new int[targetLength + 1])
				.ToArray();

			for (var i = 1; i <= source.Length; ++i)
			{
				opMatrix[i][0] = EditOperationKind.Remove;
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				opMatrix[0][i] = EditOperationKind.Add;
				costMatrix[0][i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				for (var j = 1; j <= target.Length; ++j)
				{
					var insert = costMatrix[i][j - 1] + 1;
					var delete = costMatrix[i - 1][j] + 1;
					var edit = costMatrix[i - 1][j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);

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

					costMatrix[i][j] = min;
				}
			}

			var operations = new Stack<EditOperation>(source.Length + target.Length);

			for (int x = target.Length, y = source.Length; (x > 0) || (y > 0);)
			{
				var op = opMatrix[y][x];

				if (op == EditOperationKind.Add)
				{
					x -= 1;
					operations.Push(new EditOperation('\0', target[x], op));
				}
				else if (op == EditOperationKind.Remove)
				{
					y -= 1;
					operations.Push(new EditOperation(source[y], '\0', op));
				}
				else if (op == EditOperationKind.Edit)
				{
					x -= 1;
					y -= 1;
					operations.Push(new EditOperation(source[y], target[x], op));
				}
				else // Start of the matching (EditOperationKind.None)
					break;
			}

			Debug.WriteLine($"Op Diff: {operations.Count(e => e.Operation != EditOperationKind.None)}");
			for (int i = 0, l = operations.Count; i < l; i++)
			{
				Debug.WriteLine(operations.Pop());
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
