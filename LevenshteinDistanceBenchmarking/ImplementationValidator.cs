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
		private ILevenshteinDistanceCalculator[] Implementations;

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
			Console.WriteLine("Validating calculators...");
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

		private int[] GetTestResults(ILevenshteinDistanceCalculator calculator)
		{
			var results = new List<int>();

			var testA1 = "Hello World!";
			var testA2 = "HeLLo Wolrd!";
			results.Add(calculator.CalculateDistance(testA1, testA2));

			var testE1 = "Nulla enim.";
			var testE2 = "Nulla - Hello - enim.";
			results.Add(calculator.CalculateDistance(testE1, testE2));

			var testB1 = "Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi.";
			var testB2 = "Nulla nec ipsum sit amet - Hello - enim malesuada dapibus vel quis mi.";
			results.Add(calculator.CalculateDistance(testB1, testB2));

			var testC1 = "Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi. Proin lacinia arcu non blandit mattis.";
			var testC2 = "Nulla nec ipsum sit amet enim malesuada dapibus vel quis mi. Proin lacinia arcu non blandit mattis.";
			results.Add(calculator.CalculateDistance(testC1, testC2));

			var testD1 = "Hello World!";
			var testD2 = "";
			results.Add(calculator.CalculateDistance(testD1, testD2));

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
				results.Add(calculator.CalculateDistance(comparisonString, comparisonString));
			}

			return results.ToArray();
		}
	}
}
