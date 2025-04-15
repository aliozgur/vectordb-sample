using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VectorSample.ChatService
{
    internal record ChatRequest
    {
        [JsonPropertyName("model")] 
        public string Model { get; init; } = default!;
        
        [JsonPropertyName("stream")] 
        public bool Stream { get; init; } = true;
        
        [JsonPropertyName("messages")] 
        public List<ChatMessage> Messages { get; init; } = new();
    }

}
