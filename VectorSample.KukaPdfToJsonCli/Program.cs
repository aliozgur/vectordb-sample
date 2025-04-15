using Spectre.Console;
using System.Text.Json;

namespace VectorSample.KukaPdfToJsonCli
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Please provide one or more PDF file paths.");
                return;
            }

            var pdfPath = args.Where(File.Exists).ToList().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(pdfPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] No valid PDF files provided.");
                return;
            }

            var outputFileName = args.Where(File.Exists).ToList().Skip(1).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(outputFileName))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Please provide a json file output path.");
                return;
            }

            var parser = new KukaMessageParser();

            var diagnostics = parser.Parse(pdfPath);
            string json = JsonSerializer.Serialize(diagnostics, new JsonSerializerOptions() { WriteIndented = true} );

            // Save to file
            File.WriteAllText(outputFileName, json);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
