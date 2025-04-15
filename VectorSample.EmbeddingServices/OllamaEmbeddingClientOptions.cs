using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSample.EmbeddingServices
{
    public class OllamaEmbeddingClientOptions
    {

        public string BaseUrl { get; private set; }
        public string Model { get; private set; }

        public OllamaEmbeddingClientOptions(string baseUrl = "http://localhost:11434", string model = "nomic-embed-text")
        {

            BaseUrl = baseUrl;
            Model = model;
        }

    }
}
