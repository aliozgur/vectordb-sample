using System.Net.Http.Json;

namespace VectorSample.EmbeddingServices
{
    public class OllamaEmbeddingClient: IDisposable
    {
       

        private readonly HttpClient _http = new();
        private OllamaEmbeddingClientOptions _options;
        private string _model;
        private Uri _embedUri;

        public OllamaEmbeddingClient(OllamaEmbeddingClientOptions options)
        {

            _options = options;
            _model = _options.Model;
            var baseUri = new Uri(_options.BaseUrl);
            _embedUri = new Uri(baseUri,"api/embed");
        }


        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var request = new { model = _model, input = $"{text}" };

            var response = await _http.PostAsJsonAsync(_embedUri, request);
            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();

            return result?.Embeddings?.FirstOrDefault()?.ToArray() ?? throw new Exception("No embedding returned");
        }


        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}
