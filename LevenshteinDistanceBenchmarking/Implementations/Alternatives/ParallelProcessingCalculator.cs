using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevenshteinDistanceBenchmarking.Implementations.Alternatives
{
	class ParallelProcessingCalculator : ILevenshteinDistanceMemoryCalculator
	{
		public int CalculateDistance(ReadOnlyMemory<char> source, ReadOnlyMemory<char> target)
		{
			var costMatrix = Enumerable
			  .Range(0, source.Length + 1)
			  .Select(line => new int[target.Length + 1])
			  .ToArray();

			for (var i = 1; i <= source.Length; ++i)
			{
				costMatrix[i][0] = i;
			}

			for (var i = 1; i <= target.Length; ++i)
			{
				costMatrix[0][i] = i;
			}

			var columns = target.Length + 1;
			var maxDegreeOfParallelism = Environment.ProcessorCount;
			var columnsPerParallel = (int)Math.Ceiling((double)columns / maxDegreeOfParallelism);
			var columnsLeft = columns;
			var degreeOfParallelism = 0;
			for (; columnsLeft >= columnsPerParallel && degreeOfParallelism < maxDegreeOfParallelism; columnsLeft -= columnsPerParallel, degreeOfParallelism++) ;
			if (columnsLeft > 0)
			{
				degreeOfParallelism++;
			}

			var rowProgress = new int[degreeOfParallelism];

			Parallel.For(0, degreeOfParallelism, parallelIndex =>
			{
				var localSource = source.Span;
				var localTarget = target.Span;
				var columnStartIndex = columnsPerParallel * parallelIndex + 1;

				for (var i = 1; i <= source.Length; ++i)
				{
					while (parallelIndex != 0 && rowProgress[parallelIndex - 1] <= i) ;

					var columnTravel = 0;
					for (var j = columnStartIndex; j <= target.Length && columnTravel < columnsPerParallel; ++j, columnTravel++)
					{
						var insert = costMatrix[i][j - 1] + 1;
						var delete = costMatrix[i - 1][j] + 1;
						var edit = costMatrix[i - 1][j - 1] + (localSource[i - 1] == localTarget[j - 1] ? 0 : 1);

						costMatrix[i][j] = Math.Min(Math.Min(insert, delete), edit);
					}

					rowProgress[parallelIndex] = i;
				}

				rowProgress[parallelIndex]++;
			});

			return costMatrix[source.Length][target.Length];
		}
	}
}
