using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VectorSample.ChatService
{
    internal record ChatMessage(
           [property: JsonPropertyName("role")] string Role,
           [property: JsonPropertyName("content")] string Content
       );
}
