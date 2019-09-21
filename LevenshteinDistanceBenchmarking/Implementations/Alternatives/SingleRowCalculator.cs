using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	/// <summary>
	/// Inspired by "Iosifovich" by Frederik Hertzum
	/// https://bitbucket.org/clearer/iosifovich/src/master/
	/// </summary>
	class SingleRowCalculator : ILevenshteinDistanceCalculator
	{
		public int CalculateDistance(string source, string target)
		{
			var previousRow = new int[target.Length + 1];

			for (var i = 1; i <= target.Length; ++i)
			{
				previousRow[i] = i;
			}

			for (var i = 1; i <= source.Length; ++i)
			{
				var previousDiagonal = previousRow[0];
				var previousColumn = previousRow[0]++;

				for (var j = 1; j <= target.Length; ++j)
				{
					var insertOrDelete = Math.Min(previousColumn, previousRow[j]) + 1;
					var edit = previousDiagonal + (source[i - 1] == target[j - 1] ? 0 : 1);

					previousColumn = Math.Min(insertOrDelete, edit);
					previousDiagonal = previousRow[j];
					previousRow[j] = previousColumn;
				}
			}

			return previousRow[target.Length];
		}
	}
}
