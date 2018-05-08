using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using CommandLine;
using Newtonsoft.Json;
using Retrosheet.Utilities.GameLogUtility.CommandLine;

namespace Retrosheet.Utilities.GameLogUtility
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var result = 0;

            try
            {
                result = Parser.Default
                    .ParseArguments<ProcessGameLogOptions>(args)
                    .MapResult(RunProcessGameLog, errors => 1);
            }
            catch (Exception exception)
            {
                result = 1;
                Console.WriteLine(exception);
            }

            return result;
        }

        private static int RunProcessGameLog(ProcessGameLogOptions opts)
        {
            var inputFilePaths = Directory.GetFiles(opts.InputDirectory);
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

            return 0;
        }
    }
}