using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.Alternatives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LevenshteinDistanceBenchmarking
{
	class ImplementationValidator
	{
		private readonly ILevenshteinDistanceCalculator[] Implementations;

		private List<Tuple<string, string>> ValidationData;

		public ImplementationValidator()
		{
			var engineTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => 
					t.IsClass && 
					typeof(ILevenshteinDistanceCalculator).IsAssignableFrom(t) && 
					t != typeof(LevenshteinDistanceBaseline) &&
					t != typeof(CopyMe)
				)
				.ToArray();

			Implementations = engineTypes.Select(t => Activator.CreateInstance(t) as ILevenshteinDistanceCalculator).ToArray();
		}

		public void Validate()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Levenshtein Distance Calculator Validation");

			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write("Initialising Test Data... ");
			InitialiseTestData();
			Console.WriteLine("Done!");
			Console.ResetColor();

			var baseline = new LevenshteinDistanceBaseline();
			var baselineResults = GetTestResults(baseline);

			var stopwatch = new Stopwatch();

			foreach (var calculator in Implementations)
			{
				Console.Write($"{calculator.GetType().Name}... ".PadRight(60));
				var status = true;

				stopwatch.Restart();
				var calculatorResults = GetTestResults(calculator);

				for (int i = 0, l = calculatorResults.Length; i < l; i++)
				{
					if (calculatorResults[i] != baselineResults[i])
					{
						status = false;
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine($"Failed! (Test {i + 1} - Expected: {baselineResults[i]}, Actual: {calculatorResults[i]})");
						Console.ResetColor();
						break;
					}
				}

				if (status)
				{
					stopwatch.Stop();
					Console.WriteLine($"Passed! ({stopwatch.ElapsedMilliseconds}ms)");
				}
			}
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Validation complete!");
			Console.ResetColor();
			Console.WriteLine();
		}

		private void InitialiseTestData()
		{
			ValidationData = new List<Tuple<string, string>>();

			ValidationData.Add(new Tuple<string, string>(
				"Hello World!",
				"HeLLo Wolrd!"
			));

			ValidationData.Add(new Tuple<string, string>(
				"Nulla enim.",
				"Nulla - Hello - enim."
			));

			ValidationData.Add(new Tuple<string, string>(
				"Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi.",
				"Nulla nec ipsum sit amet - Hello - enim malesuada dapibus vel quis mi."
			));

			ValidationData.Add(new Tuple<string, string>(
				"Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi. Proin lacinia arcu non blandit mattis.",
				"Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi. Proin lacinia arcu non blandit mattis."
			));

			ValidationData.Add(new Tuple<string, string>(
				"Hello World!",
				""
			));

			ValidationData.Add(new Tuple<string, string>(
				@"Hello!
This text is across multiple lines!",
				@"Yo!
This text is also across multiple lines!
Woooo!"
			));

			ValidationData.Add(new Tuple<string, string>(
				@"Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi.


Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi. Proin lacinia arcu non blandit mattis.
",
				@"
Proin lacinia arcu non blandit mattis."
			));

			//Extra long string checks
			var baseString = "abcdefghij";
			var counts = new[] { 128, 512 };
			for (int i = 0, l = counts.Length; i < l; i++)
			{
				var builder = new StringBuilder();
				for (int i2 = 0, l2 = counts[i]; i2 < l2; i2++)
				{
					builder.Append(baseString);
				}

				var comparisonString = builder.ToString();
				ValidationData.Add(new Tuple<string, string>(
					comparisonString,
					comparisonString
				));
			}
		}

		private int[] GetTestResults(ILevenshteinDistanceCalculator calculator)
		{
			var results = new List<int>();

			for (var i = 0; i < ValidationData.Count; i++)
			{
				var testPair = ValidationData[i];
				var tmpResult = -1;
				if (calculator is ILevenshteinDistanceSpanCalculator spanCalculator)
				{
					tmpResult = spanCalculator.CalculateDistance(
						testPair.Item1,
						testPair.Item2
					);
				}
				else if (calculator is ILevenshteinDistanceMemoryCalculator memoryCalculator)
				{
					tmpResult = memoryCalculator.CalculateDistance(
						testPair.Item1.AsMemory(),
						testPair.Item2.AsMemory()
					);
				}
				else
				{
					throw new InvalidOperationException(
						$"Invalid type of calculator! Must implement {nameof(ILevenshteinDistanceSpanCalculator)} or {nameof(ILevenshteinDistanceMemoryCalculator)}"
					);
				}

				results.Add(tmpResult);
			}

			return results.ToArray();
		}
	}
}
