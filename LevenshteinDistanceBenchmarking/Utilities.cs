using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LevenshteinDistanceBenchmarking
{
	public static class Utilities
	{
		private static void PrintRow<TType>(TType[] row)
		{
			for (int i = 0; i < row.Length; i++)
			{
				if (i + 1 < row.Length)
				{
					Debug.Write($"{row[i]},");
				}
				else
				{
					Debug.WriteLine(row[i]);
				}
			}
		}

		public static void PrintMatrix<TType>(TType[][] matrix)
		{
			for (int i = 0; i < matrix.Length; i++)
			{
				PrintRow(matrix[i]);
			}
		}
	}
}
