using System;
using System.IO;
using CommandLine;
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

            var header = GameLogHeaders.GetHeaderAsString();

            foreach (var inputFilePath in inputFilePaths)
            {
                if (!Directory.Exists(opts.OutputDirectory))
                {
                    Directory.CreateDirectory(opts.OutputDirectory);
                }

                var outputFilePath = Path.Combine(opts.OutputDirectory, Path.GetFileName(inputFilePath));

                using (var writer = new StreamWriter(outputFilePath))
                {
                    using (var reader = new StreamReader(inputFilePath))
                    {
                        writer.WriteLine(header);

                        while (!reader.EndOfStream)
                        {
                            writer.WriteLine(reader.ReadLine());
                        }
                    }

                    Console.WriteLine($"Processed file copied to {outputFilePath}");
                }
            }

            Console.WriteLine("Done.");

            return 0;
        }
    }
}