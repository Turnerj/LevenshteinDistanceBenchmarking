using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		public static string ReadTestData(string fileName)
		{
			return File.ReadAllText("TestData/" + fileName);
		}

		public static string BuildString(string baseString, int numberOfCharacters)
		{
			var builder = new StringBuilder(numberOfCharacters);
			var charBlocks = (int)Math.Floor((double)numberOfCharacters / baseString.Length);
			for (int i = 0, l = charBlocks; i < l; i++)
			{
				builder.Append(baseString);
			}

			var remainder = (int)((double)numberOfCharacters / baseString.Length % 1 * baseString.Length);
			builder.Append(baseString.Substring(0, remainder));

			return builder.ToString();
		}
	}
}
