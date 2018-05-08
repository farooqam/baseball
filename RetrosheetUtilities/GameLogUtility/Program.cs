using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Retrosheet.Utilities.GameLogUtility
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("USAGE: glu [input directory]");
                return;
            }

            var inputDirectory = args[0];

            try
            {
                MainAsync(inputDirectory).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private static async Task  MainAsync(string inputDirectory)
        {
            var inputFilePaths = Directory.GetFiles(inputDirectory);
            Console.WriteLine($"Found {inputFilePaths.Length} files in input directory.");

            var gameLogFactory = new GameLogFactory(GameLogHeaders.Values);

            foreach (var inputFilePath in inputFilePaths)
            {
                using (var reader = new StreamReader(inputFilePath))
                {
                    var linesRead = 0;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var gameLog = gameLogFactory.CreateGameLogFromCsv(line);
                        var json = JsonConvert.SerializeObject(gameLog, Formatting.None);

                        Console.Write($"\rProcessing {inputFilePath} ({linesRead} lines read)...");
                        linesRead++;
                    }

                    Console.Write(Environment.NewLine);
                    Console.WriteLine($"Finished processing {inputFilePath}");
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}