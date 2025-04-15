using Spectre.Console;
using VectorSample.PdfIngest;

namespace VectorSample.PdfIngestCli
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

            using var pdfIngestor = new PdfIngestor(new(_connectionString), new(_ollamaUrl, _embeddingModel));


            foreach (var file in files)
            {

                try
                {
                    AnsiConsole.MarkupLine($"[yellow]Ingesting {file} [/]");
                    await pdfIngestor.IngestAsync(file);
                    AnsiConsole.MarkupLine($"[green]Success:[/] Ingested {file}");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] Failed to ingest {file}: {ex.Message}");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
