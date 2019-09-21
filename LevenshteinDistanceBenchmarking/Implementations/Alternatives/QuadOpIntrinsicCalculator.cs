using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class QuadOpIntrinsicCalculator : ILevenshteinDistanceSpanCalculator
	{
		public unsafe int CalculateDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target)
		{
			var targetLength = target.Length;
			var costMatrix = Enumerable
			  .Range(0, source.Length + 1)
			  .Select(line => new int[targetLength + 1])
			  .ToArray();

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0][i] = i;
			}

			var allOnes256Vector = Vector256.Create(1);

			for (var i = 1; i <= source.Length; ++i)
			{
				fixed (int* prevRowPtr = costMatrix[i - 1])
				{
					for (var j = 1; j <= target.Length; j += 4)
					{
						var deletesAndEditsVector = Avx.LoadVector256(prevRowPtr + j - 1);
						deletesAndEditsVector = Avx2.Add(deletesAndEditsVector, allOnes256Vector);

						var insert1 = costMatrix[i][j - 1] + 1;
						var delete1 = deletesAndEditsVector.GetElement(1);
						var edit1 = deletesAndEditsVector.GetElement(0);

						if (source[i - 1] == target[j - 1])
						{
							edit1--;
						}

						var result1 = Math.Min(Math.Min(insert1, delete1), edit1);
						costMatrix[i][j] = result1;

						if (j == target.Length)
						{
							break;
						}

						var insert2 = result1 + 1;
						var delete2 = deletesAndEditsVector.GetElement(2);
						var edit2 = deletesAndEditsVector.GetElement(1);

						if (source[i - 1] == target[j])
						{
							edit2--;
						}

						var result2 = Math.Min(Math.Min(insert2, delete2), edit2);
						costMatrix[i][j + 1] = result2;

						if (j + 1 == target.Length)
						{
							break;
						}

						var insert3 = result2 + 1;
						var delete3 = deletesAndEditsVector.GetElement(3);
						var edit3 = deletesAndEditsVector.GetElement(2);

						if (source[i - 1] == target[j + 1])
						{
							edit3--;
						}

						var result3 = Math.Min(Math.Min(insert3, delete3), edit3);
						costMatrix[i][j + 2] = result3;

						if (j + 2 == target.Length)
						{
							break;
						}

						var insert4 = result3 + 1;
						var delete4 = deletesAndEditsVector.GetElement(4);
						var edit4 = deletesAndEditsVector.GetElement(3);

						if (source[i - 1] == target[j + 2])
						{
							edit4--;
						}

						var result4 = Math.Min(Math.Min(insert4, delete4), edit4);
						costMatrix[i][j + 3] = result4;
					}
				}
			}

			return costMatrix[source.Length][target.Length];
		}
	}
}
