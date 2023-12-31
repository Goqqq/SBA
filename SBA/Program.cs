using SBA.Modell;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.IO;
using System.Diagnostics;

namespace SBA;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	private static UniqueIDGenerator idGenerator = new();

	[STAThread]
	public static void Main(String[] args)
	{
		int[] numberOfShipsValues = { 5, 15, 25 };
		int minLength = 5;
		int maxLength = 10;
		int minLayTime = 3;
		int maxLayTime = 7;
		int numberOfInstancesPerValue = 5;
		int[] berthLengthValues = { 5, 10, 15 };
		int seed = 42;

		//string hintPath = @"..\..\..\..\..\..\gurobi1001\win64\bin\Gurobi100.NET.dll";

		//if (System.IO.File.Exists(hintPath))
		//{
		//	Console.WriteLine("Path is valid.");
		//}
		//else
		//{
		//	Console.WriteLine("Path is not valid.");
		//}


		UniqueIDGenerator idGenerator = new();

		BerthAllocationInstance.GenerateInstances(
			numberOfShipsValues,
			minLength,
			maxLength,
			minLayTime,
			maxLayTime,
			berthLengthValues,
			numberOfInstancesPerValue,
			seed
		);
		String a = ("Current Directory: " + AppDomain.CurrentDomain.BaseDirectory);
		string[] filePaths = Directory.GetFiles(
			Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Instance\Data"),
			"*.txt"
		);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		int counter = 0;



		
		// Create a file to write the stopwatch results
		string stopwatchResultFilePath = Path.Combine(@"..\..\..\results", "benchmarking_results.txt");

		
		Stopwatch stopwatch = new Stopwatch(); // for measuring time --> Benchmarking

		using (StreamWriter stopwatchWriter = new StreamWriter(stopwatchResultFilePath))
		{
		    
			foreach (string filePath in filePaths)
			{
				if (!File.Exists(filePath))
				{
					Console.WriteLine($"File not found: {filePath}");
					continue; // Skip this file and continue with the next one.
				}
				stopwatch.Start();	// Start the stopwatch
				BerthAllocationInstance instance = BerthAllocationInstance.ReadInstances(filePath);

				//stopwatch.Restart(); // Start the stopwatch

				Problem problem = new(instance.Quay, new List<Ship>(instance.Ships));
				Allocator allocator = new(problem.Ships, problem.Quay);
				allocator.AllocateShips();
				List<Solution> solution = allocator.GetSolution();

				//stopwatch.Stop(); // Stop the stopwatch 
				//stopwatchWriter.WriteLine($"Instance {counter}: Time taken to solve instance: {stopwatch.ElapsedMilliseconds} ms");
				// Console.WriteLine($"Time taken to solve instance: {stopwatch.ElapsedMilliseconds} ms");

				// Draw the form and save the result as an image
				using (SBA form = new SBA(solution, instance.Quay.Capacity))
				{
					//form.ShowDialog();
					form.SaveDrawingAsImage($"result{counter}");
				}

				GurobiOptimizer grb = new GurobiOptimizer(problem);

				

				List<Solution> grb_solution = grb.Solve();

				stopwatch.Stop(); // Stop the stopwatch for the GurobiOptimizer
				stopwatchWriter.WriteLine($"Instance {counter}: Time taken to solve instance: {stopwatch.ElapsedMilliseconds} ms");

				// Draw the form and save the result as an image
				using (SBA form = new SBA(grb_solution, instance.Quay.Capacity))
				{
					//form.ShowDialog();
					form.SaveDrawingAsImage($"result_gurobi{counter}");
				}
				UniqueIDGenerator.ResetCounter();
				counter++;

				

			}
			
		}
		//Quay quay = new Quay(10);
		//      List<Ship> ships = new();
		//      Ship ship1 = new(4, 6);
		//      Ship ship2 = new(2, 3);
		//      Ship ship3 = new(3, 4);
		//      Ship ship4 = new(5, 5);
		//      Ship ship5 = new(6, 4);
		//      //Ship ship6 = new(7, 7);
		//      //Ship ship6 = new(6, 5);
		//      ships.Add(ship1);
		//      ships.Add(ship2);
		//      ships.Add(ship3);
		//      ships.Add(ship4);
		//      ships.Add(ship5);
		//      //ships.Add(ship6);
		//      Problem problem = new(quay, ships);
		//      //ships.Add(ship6);
		//      Allocator allocator = new(problem.Ships, problem.Quay);
		//      allocator.AllocateShips();
		//      allocator.PrintSolution();
		//      List<Solution> solution = allocator.GetSolution();

		//      Application.EnableVisualStyles();
		//      Application.SetCompatibleTextRenderingDefault(false);
		//      Application.Run(new SBA(solution, quay.Capacity));
	}

	//private static void DrawAndSaveSolution(
	//    BerthAllocationInstance instance,
	//    List<Solution> solution
	//)
	//{
	//    using (SBA form = new SBA(solution, instance.Quay.Capacity))
	//    {
	//        // Draw the form and show it
	//        form.ShowDialog();
	//        // Save the form's content as an image
	//        Bitmap bmp = new Bitmap(form.Width, form.Height);
	//        form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
	//        bmp.Save($"solution_instance_{idGenerator.GenerateID}.png", ImageFormat.Png);
	//    }
	//}
}
