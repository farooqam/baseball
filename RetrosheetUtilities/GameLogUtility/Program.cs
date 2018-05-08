using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Retrosheet.Utilities.GameLogUtility
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("USAGE: glu [input directory] [Cosmos db endpoint] [Cosmos db auth key] [Database name]");
                return;
            }

            var inputDirectory = args[0];
            var endpoint = args[1];
            var authKey = args[2];
            var database = args[3];

            var documentClient = new DocumentClient(new Uri(endpoint), authKey);

            try
            {
                MainAsync(inputDirectory, documentClient, database).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            finally
            {
                documentClient.Dispose();  
            }
        }

        private static async Task  MainAsync(string inputDirectory, DocumentClient client, string database)
        {
            var inputFilePaths = Directory.GetFiles(inputDirectory);
            Console.WriteLine($"Found {inputFilePaths.Length} files in input directory.");

            var gameLogFactory = new GameLogFactory(GameLogHeaders.Values);
            var docCollectionUri = UriFactory.CreateDocumentCollectionUri(database, "gamelog");

            foreach (var inputFilePath in inputFilePaths)
            {
                var gameLogs = new List<ExpandoObject>();

                using (var reader = new StreamReader(inputFilePath))
                {
                    var linesRead = 0;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var gameLog = gameLogFactory.CreateGameLogFromCsv(line);
                        gameLogs.Add(gameLog);

                        Console.Write($"\rProcessing {inputFilePath} ({linesRead} lines read)...");
                        linesRead++;
                    }

                    Console.Write(Environment.NewLine);
                    Console.WriteLine($"Finished processing {inputFilePath}");

                    Console.WriteLine("Uploading documents.");

                    foreach (var gameLog in gameLogs)
                    {
                        var response = await client.UpsertDocumentAsync(docCollectionUri, gameLog);
                        Console.WriteLine($"Document uploaded to {response.Resource.SelfLink}; Status:{response.StatusCode}; RU's:{response.RequestCharge}");
                    }
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}