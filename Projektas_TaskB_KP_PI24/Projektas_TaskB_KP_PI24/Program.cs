using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics;

namespace Projektas_TaskB_KP_PI24
{
	internal static class Program
	{
		static void Main()
		{
			const string FILES_FOLDER = @"D:\uni_projects\multithreated\Projektas_TaskB_KP_PI24\Projektas_TaskB_KP_PI24\Files";
			
			bool running = true;
			string[] files = Directory.GetFiles(FILES_FOLDER)
			.Where(file => Path.GetFileName(file) != "Results.txt")
			.ToArray();

			while (running) {
				Console.WriteLine("---------------------------");
				Console.WriteLine("Main menu");
				Console.WriteLine("1. Shows results.");
				Console.WriteLine("2. Search in files.");
				Console.WriteLine("3. Close the application.");
				Console.WriteLine("Choose an option:");

				string input = Console.ReadLine();

				switch (input) {
					case "1":
						string resultsFile = Path.Combine(FILES_FOLDER, "Results.txt");
						Console.WriteLine("Results.txt:");

						if (File.Exists(resultsFile))
						{
							string fileData = File.ReadAllText(resultsFile);
							Console.WriteLine(fileData);
						}
						else
						{
							Console.WriteLine("Results.txt does not exist yet.");
						}
						break;

					case "2":
						string path = @"D:\\\\uni_projects\\\\multithreated\\\\Projektas_TaskB_KP_PI24\\\\Projektas_TaskB_KP_PI24\\\\Files\\\\Results.txt";
						
						int totalOfMatches = 0;
						ConcurrentBag<string> results = new ConcurrentBag<string>();
						ConcurrentBag<long> threadTimes = new ConcurrentBag<long>();

						Console.WriteLine("Input what to search for:");
						string search = Console.ReadLine();

						Console.WriteLine("Matching files:");
						CountdownEvent countdown = new CountdownEvent(files.Length);

						foreach (string file in files)
						{
							ThreadPool.QueueUserWorkItem(_ =>
							{
								try
								{
									Stopwatch stopwatch = Stopwatch.StartNew();

									int threadId = Environment.CurrentManagedThreadId;
									string fileName = Path.GetFileName(file);

									Console.WriteLine($"Thread {threadId} - {fileName} - Work started");

									string content = File.ReadAllText(file);

									if (content.Contains(search, StringComparison.OrdinalIgnoreCase))
									{
										Console.WriteLine($"Thread {threadId} - {fileName} - Found match: {search}");
										Interlocked.Increment(ref totalOfMatches);
										results.Add(fileName);
									}

									stopwatch.Stop();
									threadTimes.Add(stopwatch.ElapsedMilliseconds);

									Console.WriteLine($"Thread {threadId} - {fileName} - Work finished in {stopwatch.ElapsedMilliseconds} ms");
								}
								finally
								{
									countdown.Signal();
								}
							});
						}

						countdown.Wait();

						double averageTime = threadTimes.Average();

						Console.WriteLine($"Average file processing time: {averageTime:F2} ms");

						List<string> output = new List<string>();
						output.Add($"Search value: {search}");
						output.Add("Matching files:");
						output.AddRange(results);
						output.Add($"Total files matching: {totalOfMatches}");
						output.Add($"Average file processing time: {averageTime:F2} ms");

						File.WriteAllLines(path, output);
						Console.WriteLine($"Total files matching: {totalOfMatches}");
						break;

					case "3":
						Console.WriteLine("Menu is closed.");
						running = false;
						break;

					default:
						Console.WriteLine("There's no such option, try again.");
						break;
				}
			}
		}
	}
}