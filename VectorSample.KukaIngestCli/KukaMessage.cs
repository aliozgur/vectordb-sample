namespace VectorSample.KukaIngestCli
{

    public class KukaMessage
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public string Cause { get; set; }
        public string Effect { get; set; }
        public string Remedy { get; set; }
    }

    public static class KukaMessageExtensions
    {
        public static string ToEmbeddingPrompt(this KukaMessage msg)
        {
            return $"""
                Code: {msg.Code} (This is used as message code or error code)
                Title: {msg.Title} (This is the title of the code)
                Cause: {msg.Cause} 
                Effect: {msg.Effect}
                Remedy: {msg.Remedy} (This is remedy or correction action)
                """;
        }

        public static Dictionary<string, string> ToMetadata(this KukaMessage msg)
        {
            return new Dictionary<string, string>
            {
                ["Code"] = msg.Code.ToString(),
                ["Title"] = msg.Title ?? string.Empty,
                ["Cause"] = msg.Cause ?? string.Empty,
                ["Effect"] = msg.Effect ?? string.Empty,
                ["Remedy"] = msg.Remedy ?? string.Empty
            };
        }
    }
}
