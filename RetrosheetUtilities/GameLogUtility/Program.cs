using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Retrosheet.Utilities.GameLogUtility
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 4 || args.Length > 5)
            {
                Console.WriteLine("USAGE: glu [input directory] [Cosmos db endpoint] [Cosmos db auth key] [Database name] (compress - optional)");
                return;
            }

            var inputDirectory = args[0];
            var endpoint = args[1];
            var authKey = args[2];
            var database = args[3];
            var compress = false;

            if (args.Length == 5)
            {
                if (string.Compare(args[4], "compress", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    Console.WriteLine("USAGE: glu [input directory] [Cosmos db endpoint] [Cosmos db auth key] [Database name] (compress - optional)");
                    return;
                }
                else
                {
                    compress = true;
                }
            }

            var documentClient = new DocumentClient(new Uri(endpoint), authKey);

            try
            {
                MainAsync(inputDirectory, documentClient, database, compress).GetAwaiter().GetResult();
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

        private static async Task MainAsync(string inputDirectory, DocumentClient client, string database, bool compress)
        {
            var inputFilePaths = Directory.GetFiles(inputDirectory);
            Console.WriteLine($"Found {inputFilePaths.Length} files in input directory.");

            var gameLogFactory = new GameLogFactory(GameLogHeaders.Values);
            var docCollectionUri = UriFactory.CreateDocumentCollectionUri(database, "gamelog");
            var stringCompressor = new StringCompressor();

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

                    var docsUploaded = 0;

                    foreach (var gameLog in gameLogs)
                    {
                        var dyno = (dynamic) gameLog;
                        var docId = $"{dyno.game_year}-{dyno.game_month}-{dyno.game_day}-{dyno.game_number}-{dyno.home_team}";
                        ResourceResponse<Document> response = null;

                        if (compress)
                        {
                            var payload = new
                            {
                                id = docId,
                                body = stringCompressor.Compress(JsonConvert.SerializeObject(gameLog))
                            };

                            response = await client.UpsertDocumentAsync(docCollectionUri, payload);
                        }
                        else
                        {
                            var payload = new
                            {
                                id = docId,
                                body = gameLog
                            };

                            response = await client.UpsertDocumentAsync(docCollectionUri, payload);
                        }

                        docsUploaded++;
                        Console.Write($"\rDocument {docsUploaded}/{gameLogs.Count} uploaded; Status:{response.StatusCode}; RU's:{response.RequestCharge}");
                        Console.WriteLine(Environment.NewLine);
                    }
                }
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }
    }
}