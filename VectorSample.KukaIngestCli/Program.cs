using Spectre.Console;
using System.Text.Json;
using VectorSample.Database;
using VectorSample.EmbeddingServices;

namespace VectorSample.KukaIngestCli
{
    internal class Program
    {

        private static string _connectionString = "Host=localhost;Port=54321;Username=admin;Password=admin;Database=vectordb";
        private static string _embeddingModel = "nomic-embed-text";
        private static string _ollamaUrl = "http://localhost:11434";




        static async Task Main(string[] args)
        {

            if (args.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Please provide one or more PDF file paths.");
                return;
            }

            var files = args.Where(File.Exists).ToList();
            if (!files.Any())
            {
                AnsiConsole.MarkupLine("[red]Error:[/] No valid PDF files provided.");
                return;
            }


            using var ollamaClient = new OllamaEmbeddingClient(new(_ollamaUrl, _embeddingModel));
            using var vectorStore = new VectorStore(new(_connectionString));

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var kukaMessages = JsonSerializer.Deserialize<List<KukaMessage>>(json);
                var total = kukaMessages.Count;
                var cnt = 0;
                
                List <(float[] vector, KukaMessage message)> values = new List<(float[] vector, KukaMessage message)>();

                foreach (var message in kukaMessages)
                {
                    cnt++;
                    try
                    {

                        AnsiConsole.MarkupLine($"[yellow]({cnt}/{total}) Generating embedding from {file} [/]");

                        var vector = await ollamaClient.GetEmbeddingAsync(message.ToEmbeddingPrompt());
                        values.Add((vector, message));

                        AnsiConsole.MarkupLine($"[yellow]({cnt}/{total}) Storing embeding from {file} [/]");

                        await vectorStore.StoreDocumentAsync(message.ToEmbeddingPrompt(), vector, message.ToMetadata());


                        AnsiConsole.MarkupLine($"[green]({cnt}/{total}) Ingested {file}[/]");
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]Error:[/] Failed to ingest {file}: {ex.Message}");
                    }
                    
                }

                //var embedTasks = kukaMessages.Select(async (message, index) =>
                //{
                //    try
                //    {
                //        var cnt = index + 1;
                //        AnsiConsole.MarkupLine($"[yellow]({cnt}/{total}) Generating embedding from {file} [/]");

                //        var vector = await ollamaClient.GetEmbeddingAsync(message.ToEmbeddingPrompt());
                //        values.Add((vector, message));
                //    }
                //    catch (Exception ex)
                //    {
                //        AnsiConsole.MarkupLine($"[red]Error:[/] Failed to ingest {file}: {ex.Message}");
                //    }
                //});

                //await Task.WhenAll(embedTasks);


                //var storageTasks = values.Select(async v =>
                //{
                //    try
                //    {
                //        AnsiConsole.MarkupLine($"[yellow]({cnt}/{total}) Storing embeding from {file} [/]");
                //        await vectorStore.StoreDocumentAsync(v.message.ToEmbeddingPrompt(),v.vector, v.message.ToMetadata());
                //        AnsiConsole.MarkupLine($"[green]({cnt}/{total}) Ingested {file}[/]");
                //    }
                //    catch (Exception ex)
                //    {
                //        AnsiConsole.MarkupLine($"[red]Error:[/] Failed to ingest {file}: {ex.Message}");
                //    }
                //});

                //await Task.WhenAll(storageTasks);

            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

        }
    }
}
