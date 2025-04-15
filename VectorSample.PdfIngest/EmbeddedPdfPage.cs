using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorSample.PdfIngest
{
    public record EmbeddedPdfPage
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public int PageNumber { get; init; }
        public string Content { get; init; } = string.Empty;
        public float[] Embedding { get; init; } = Array.Empty<float>();
        public string FileName { get; init; } = string.Empty;
    }

}
