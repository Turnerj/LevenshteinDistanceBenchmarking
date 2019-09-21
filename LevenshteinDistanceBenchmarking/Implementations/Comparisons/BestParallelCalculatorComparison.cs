using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LevenshteinDistanceBenchmarking.Implementations.Comparisons
{
	class BestParallelCalculatorComparison : ILevenshteinDistanceMemoryCalculator
	{
		private class TaskData
		{
			public int Row;
			public bool ReadFirstChar;
			public AutoResetEvent Event;
		}

		public int CalculateDistance(ReadOnlyMemory<char> source, ReadOnlyMemory<char> target)
		{
			var sourceLength = source.Length;
			var targetLength = target.Length;
			var columns = targetLength + 1;

			var arrayPool = ArrayPool<int>.Shared;
			var rentedPool = arrayPool.Rent(2 * columns);
			var costMatrix = new Span<int>(rentedPool);

			for (var i = 1; i <= targetLength; ++i)
			{
				costMatrix[i] = i;
			}

			costMatrix[0] = 0;

			var maxDegreeOfParallelism = Environment.ProcessorCount;
			var columnsPerParallel = (int)Math.Ceiling((double)columns / maxDegreeOfParallelism);
			columnsPerParallel = Math.Max(columnsPerParallel, 16);
			var columnsLeft = columns;
			var degreeOfParallelism = 0;
			for (; columnsLeft >= columnsPerParallel && degreeOfParallelism < maxDegreeOfParallelism; columnsLeft -= columnsPerParallel, degreeOfParallelism++) ;
			if (columnsLeft > 0)
			{
				degreeOfParallelism++;
			}

			var parallelData = new TaskData[degreeOfParallelism];
			for (var i = 0; i < degreeOfParallelism; i++)
			{
				parallelData[i] = new TaskData
				{
					Row = 0,
					ReadFirstChar = false,
					Event = new AutoResetEvent(false)
				};
			}


			Parallel.For(0, degreeOfParallelism, parallelIndex =>
			{
				var localSource = source.Span;
				var localTarget = target.Span;
				var localCostMatrix = new Span<int>(rentedPool);

				var columnStartIndex = columnsPerParallel * parallelIndex + 1;
				var currentTaskData = parallelData[parallelIndex];

				for (var i = 1; i <= sourceLength; ++i)
				{
					var currentRow = localCostMatrix.Slice((i & 1) * columns);

					if (parallelIndex == 0)
					{
						currentRow[0] = i;
					}

					if (degreeOfParallelism > 0)
					{
						var isNotFirstThread = parallelIndex > 0;
						var isNotLastThread = parallelIndex + 1 < degreeOfParallelism;
						var prevTaskData = isNotFirstThread ? parallelData[parallelIndex - 1] : null;
						var nextTaskData = isNotLastThread ? parallelData[parallelIndex + 1] : null;

						var currentRowNumber = currentTaskData.Row;
						var previousRowNumber = currentRowNumber - 1;

						while (
							//Previous task isn't ready for current task to continue
							(
								isNotFirstThread &&
								prevTaskData.Row <= currentRowNumber
							) ||
							//Next task isn't ready for current task to continue
							(
								isNotLastThread &&
								(
									(
										nextTaskData.Row == previousRowNumber &&
										!nextTaskData.ReadFirstChar
									) ||
									nextTaskData.Row < previousRowNumber
								)
							)
						)
						{
							//currentTaskData.Event.WaitOne();
						}
					}

					var previousRow = localCostMatrix.Slice(((i - 1) & 1) * columns);
					var sourcePrevChar = localSource[i - 1];
					var columnTravel = 0;

					for (var j = columnStartIndex; j <= targetLength && columnTravel < columnsPerParallel; j += 2, columnTravel += 2)
					{
						var insert1 = currentRow[j - 1] + 1;
						var delete1 = previousRow[j] + 1;
						var edit1 = previousRow[j - 1] + (sourcePrevChar == localTarget[j - 1] ? 0 : 1);

						var result1 = Math.Min(Math.Min(insert1, delete1), edit1);
						currentRow[j] = result1;

						if (!currentTaskData.ReadFirstChar)
						{
							currentTaskData.ReadFirstChar = true;
						}

						if (j == targetLength || columnTravel + 1 == columnsPerParallel)
						{
							break;
						}

						var insert2 = result1 + 1;
						var delete2 = previousRow[j + 1] + 1;
						var edit2 = previousRow[j] + (sourcePrevChar == localTarget[j] ? 0 : 1);

						var result2 = Math.Min(Math.Min(insert2, delete2), edit2);
						currentRow[j + 1] = result2;
					}

					currentTaskData.ReadFirstChar = false;
					currentTaskData.Row++;
				}
			});

			var result = costMatrix[(sourceLength & 1) * columns + targetLength];
			arrayPool.Return(rentedPool);
			return result;
		}
	}
}
