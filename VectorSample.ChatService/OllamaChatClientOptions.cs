using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSample.ChatService
{
    public class OllamaChatClientOptions
    {

        public string BaseUrl { get; private set; }
        public string Model { get; private set; }

        public OllamaChatClientOptions(string baseUrl = "http://localhost:11434", string model = "phi4")
        {

            BaseUrl = baseUrl;
            Model = model;
        }

    }
}
