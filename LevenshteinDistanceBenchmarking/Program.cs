using BenchmarkDotNet.Running;
using System;

namespace LevenshteinDistanceBenchmarking
{
	class Program
	{
		static void Main(string[] args)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
		}
	}
}
