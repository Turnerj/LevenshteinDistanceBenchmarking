using LevenshteinDistanceBenchmarking.Implementations;
using LevenshteinDistanceBenchmarking.Implementations.IsolatedImprovements;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LevenshteinDistanceBenchmarking.Validation
{
	class ImplementationValidator
	{
		private readonly ILevenshteinDistanceCalculator[] Implementations;

		private List<Tuple<string, string>> ValidationData;

		public ImplementationValidator()
		{
			var implementationAssembly = typeof(Utilities).Assembly;
			var engineTypes = implementationAssembly.GetTypes()
				.Where(t =>
					t.IsClass &&
					typeof(ILevenshteinDistanceCalculator).IsAssignableFrom(t) &&
					t != typeof(LevenshteinDistanceBaseline) &&
					t != typeof(LevenshteinDistanceDebugger) &&
					t != typeof(CopyMe)
				)
				.ToArray();

			Implementations = engineTypes.Select(t => Activator.CreateInstance(t) as ILevenshteinDistanceCalculator).ToArray();
			//Implementations = new[] { new Implementations.Frontends.MultiLineSubsectionFrontend() };
		}

		private void WriteTestRunning(string implementation)
		{
			ClearLine();
			Console.Write($"[RUNNING] {implementation}...");
		}
		private void WriteTestSuccess(string implementation, TimeSpan time)
		{
			ClearLine();
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Write($"[PASSED!] {implementation}... ".PadRight(65));
			Console.WriteLine($"{time.TotalMilliseconds:0.00}ms".PadLeft(14));
			Console.ResetColor();
		}
		private void WriteTestError(string implementation, string errorMessage)
		{
			ClearLine();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write($"[FAILED!] {implementation}... ".PadRight(65));
			Console.WriteLine(errorMessage);
			Console.ResetColor();
		}

		private void ClearLine()
		{
			Console.Write("\r" + new string(' ', Console.BufferWidth) + "\r");
		}

		public void Validate()
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("Levenshtein Distance Calculator Validation");

			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write("Initialising Test Data... ");
			InitialiseTestData();

			var baseline = new LevenshteinDistanceBaseline();
			var baselineResults = GetTestResults(baseline);

			Console.WriteLine("Done!");
			Console.ResetColor();

			var stopwatch = new Stopwatch();

			foreach (var calculator in Implementations)
			{
				var implementationName = calculator.GetType().Name;
				WriteTestRunning(implementationName);
				var status = true;

				stopwatch.Restart();
				var calculatorResults = GetTestResults(calculator);

				for (int i = 0, l = calculatorResults.Length; i < l; i++)
				{
					if (calculatorResults[i] != baselineResults[i])
					{
						status = false;
						WriteTestError(implementationName, $"Test {(i + 1):00} [Expected: {baselineResults[i]}, Actual: {calculatorResults[i]}]");
						break;
					}
				}

				if (status)
				{
					stopwatch.Stop();
					WriteTestSuccess(implementationName, stopwatch.Elapsed);
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

			ValidationData.Add(new Tuple<string, string>(
				Utilities.ReadTestData("MultilineLipsum1a.txt"),
				Utilities.ReadTestData("MultilineLipsum1b.txt")
			));

			ValidationData.Add(new Tuple<string, string>(
				Utilities.ReadTestData("MultilineLipsum2a.txt"),
				Utilities.ReadTestData("MultilineLipsum2b.txt")
			));
			ValidationData.Add(new Tuple<string, string>(
				Utilities.ReadTestData("TestHtml1a.html"),
				Utilities.ReadTestData("TestHtml1b.html")
			));

			//Extra long, single line strings
			var baseString = "abcdefghij";
			var counts = new[] { 128, 512 };
			for (int i = 0, l = counts.Length; i < l; i++)
			{
				var comparisonString = Utilities.BuildString(baseString, counts[i]);
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
