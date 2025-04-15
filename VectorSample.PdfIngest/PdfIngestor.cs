using UglyToad.PdfPig;
using VectorSample.Database;
using VectorSample.EmbeddingServices;
using VectorSample.EmbeddingServices;

namespace VectorSample.PdfIngest
{
    public class PdfIngestor: IDisposable
    {
        private OllamaEmbeddingClient _embeddingClient;
        private VectorStore _vectorStore;
        public PdfIngestor(VectorStoreOptions vectoreStoreOptions, OllamaEmbeddingClientOptions ollamaEmbeddingClientOptions)
        {
           
            _embeddingClient = new OllamaEmbeddingClient(ollamaEmbeddingClientOptions);
            _vectorStore = new VectorStore(vectoreStoreOptions);
        }

        public async Task IngestAsync(string pdfPath, int? skipPages=null)
        {
            var pages = ExtractPages(pdfPath);
            pages = skipPages > 0 ? pages.Skip(skipPages.Value).ToList() : pages;

            var exceptions = new List<Exception>();


            foreach(var page in pages)
            {
                try
                {
                    Console.WriteLine($"Processing embedding for page {page.PageNumber} of {page.FileName}");
                    var embedding = await _embeddingClient.GetEmbeddingAsync(page.Content);
                    var metadata = new Dictionary<string, string>
                    {
                        { "FileName", page.FileName },
                        { "PageNumber", page.PageNumber.ToString() }
                    };

                    Console.WriteLine($"Storing page {page.PageNumber} of {page.FileName}");
                    await _vectorStore.StoreDocumentAsync(page.Content, embedding, metadata);
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(new Exception($"Page {page.PageNumber} failed", ex));
                    }
                }
            }

            //var tasks = pages.Select(async p =>
            //{
            //    try
            //    {
            //        var embedding = await _embeddingClient.GetEmbeddingAsync(p.Content);
            //        var metadata = new Dictionary<string, string>
            //        {
            //            { "FileName", p.FileName },
            //            { "PageNumber", p.PageNumber.ToString() }
            //        };

            //        await _vectorStore.StoreDocumentAsync(p.Content, embedding, metadata);
            //    }
            //    catch (Exception ex)
            //    {
            //        lock (exceptions)
            //        {
            //            exceptions.Add(new Exception($"Page {p.PageNumber} failed", ex));
            //        }
            //    }
            //});

            //await Task.WhenAll(tasks);

            if (exceptions.Any())
            {
                throw new AggregateException("One or more pages failed", exceptions);
            }
        }


        public List<EmbeddedPdfPage> ExtractPages(string filePath)
        {
            var result = new List<EmbeddedPdfPage>();
            using var doc = PdfDocument.Open(filePath);

            for (int i = 0; i < doc.NumberOfPages; i++)
            {
                var page = doc.GetPage(i + 1);
                var words = page.GetWords();
                result.Add(new EmbeddedPdfPage
                {
                    PageNumber = i + 1,
                    Content = string.Join(" ",words),
                    FileName = Path.GetFileName(filePath)
                });
            }

            return result;
        }


        public void Dispose()
        {
            _embeddingClient?.Dispose();
            _vectorStore?.Dispose();
        }
    }
}
