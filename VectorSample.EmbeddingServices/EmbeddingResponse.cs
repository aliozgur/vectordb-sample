namespace VectorSample.EmbeddingServices
{
    public record EmbeddingResponse
    {
        public string Model { get; init; }
        public List<List<float>> Embeddings { get; init; }
        public long TotalDuration { get; init; }
        public long LoadDuration { get; init; }
        public int PromptEvalCount { get; init; }
    }
}
