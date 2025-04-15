using System.Text;
using System.Text.Json;

namespace VectorSample.ChatService
{

    public class OllamaChatClient: IDisposable
    {
        private readonly HttpClient _http = new();
        private readonly OllamaChatClientOptions _options;
        private readonly string _model;
        private Uri _chatUri;

        public OllamaChatClient(OllamaChatClientOptions options)
        {

            _options = options;
            _model = _options.Model;
            var baseUri = new Uri(_options.BaseUrl);
            _chatUri = new Uri(baseUri, "api/chat");
        }

        public async IAsyncEnumerable<string> ChatAsync(string userMessage, string? systemPrompt = null, bool streaming = true)
        {
            var request = new ChatRequest
            {
                Model = _model,
                Stream = streaming,
                Messages = new List<ChatMessage>
                    {
                        new("system", systemPrompt ?? "You are a helpful assistant."),
                        new("user", userMessage)
                    }
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _chatUri)
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            var response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

               
                var chunk = JsonSerializer.Deserialize<ChatResponseChunk>(line);
                
                if (chunk?.Message?.Content != null)
                {
                    yield return chunk.Message.Content;
                }

                if (chunk?.Done == true)
                {
                    break;
                }
            }
        }

        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}
