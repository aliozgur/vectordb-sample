using Spectre.Console;
using VectorSample.ChatService;
using VectorSample.Database;
using VectorSample.EmbeddingServices;

namespace VectorSample.RagCli
{
    internal class Program
    {

        private static string _connectionString = "Host=localhost;Port=54321;Username=admin;Password=admin;Database=vectordb";
        private static string _chatModel = "qwen2.5:1.5b";
        private static string _embedModel = "nomic-embed-text";

        private static string _ollamaUrl = "http://10.16.16.13:11434";


        static async Task Main(string[] args)
        {
            
            using var vectorStore = new VectorStore(new(_connectionString));
            using var chatClient = new OllamaChatClient(new(_ollamaUrl, _chatModel));
            using var embeddingClient = new OllamaEmbeddingClient(new(_ollamaUrl, _embedModel));

            Console.WriteLine("Enter your question (or 'exit' to quit):");


            while (true)
            {

                var firstChunkArrived = false;
                var spinnerFinished = false;
                Console.WriteLine();
                var question = AnsiConsole.Prompt(
                   new TextPrompt<string>("[bold yellow]Question[/]:").AllowEmpty());

                if (string.Equals(question, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                List<string> topDocs = new();

                StatusContext statusContext = null!;

                var _ = AnsiConsole.Status()
                    .Spinner(Spinner.Known.Star)
                    .StartAsync("Thinking...", async ctx => {

                        statusContext = ctx;
                    

                        while (!firstChunkArrived)
                        {
                            await Task.Delay(100);
                        }

                        ctx.Status("Done Thinking");
                        ctx.Refresh();
                        spinnerFinished = true;
                        return;
                    });




                statusContext.Status("Generating embedding...");
                statusContext.Refresh();
                var questionVector = await embeddingClient.GetEmbeddingAsync(question);

                statusContext.Status("Searching vector store...");
                statusContext.Refresh();

                topDocs = await vectorStore.SearchAsync(questionVector, 5);
                statusContext.Status("Thinking...");
                statusContext.Refresh();

                var prompt = $"""
                    You are a helpful assistant. Use the following documents to answer the question.

                    Documents:
                    {string.Join("\n---\n", topDocs)}

                    Question: {question}
                    Answer:
                    """;


                try
                {
                    await foreach (var response in chatClient.ChatAsync(question, systemPrompt: prompt))
                    {
                        if (!firstChunkArrived)
                        {
                            AnsiConsole.MarkupLine("[bold blue]Response:[/]");
                            firstChunkArrived = true;
                        }

                        while (!spinnerFinished)
                        {
                            await Task.Delay(100);
                        }
                        Console.Write($"{response}");
                    }
                }
                catch(Exception ex)
                {
                    firstChunkArrived = true;
                    while (!spinnerFinished)
                    {
                        await Task.Delay(100);
                    }
                    AnsiConsole.MarkupLine($"[red]Error:{Markup.Escape(ex.Message)}[/]");
                }
                Console.WriteLine();
            }
        }
    }
}
