using Npgsql;
using NpgsqlTypes;
using Pgvector;
using System;
using System.Text.Json;

namespace VectorSample.Database
{
    public class VectorStore: IDisposable
    {
        private string _connStr;
        private readonly VectorStoreOptions _options;

        public VectorStore(VectorStoreOptions options)
        {
            _connStr = options.ConnectionString;
            _options = options;
        }

        public async Task StoreDocumentAsync(string content, float[] vector, IDictionary<string, string> metadata)
        {

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connStr);
            dataSourceBuilder.UseVector();
            await using var dataSource = dataSourceBuilder.Build();

            using var conn = dataSource.CreateConnection();
            try
            {
                await conn.OpenAsync();

                await using var cmd = new NpgsqlCommand("""
                INSERT INTO documents (id, content, embedding, metadata)
                VALUES (@id, @content, @embedding, @metadata)
            """, conn);

                cmd.Parameters.AddWithValue("id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("content", content);
                cmd.Parameters.AddWithValue("embedding", vector);
                // Properly defined JSONB param
                cmd.Parameters.Add(new NpgsqlParameter("metadata", JsonSerializer.Serialize(metadata))
                {
                    NpgsqlDbType = NpgsqlDbType.Jsonb
                });

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public async Task<List<string>> SearchAsync(float[] queryVector,int topK = 5)
        {

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connStr);
            dataSourceBuilder.UseVector();
            await using var dataSource = dataSourceBuilder.Build();
            

            var vector = new Pgvector.Vector(queryVector);
            using var conn = dataSource.CreateConnection();

            try
            {
                await conn.OpenAsync();
             
                await using var cmd = new NpgsqlCommand("""
                    SELECT 
                        content
                    FROM documents
                    ORDER BY embedding <=> @embedding
                    LIMIT @topK
                """, conn);

                cmd.Parameters.AddWithValue("embedding", vector);
                cmd.Parameters.AddWithValue("topK", topK);


                var results = new List<string>();
                var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    results.Add(reader.GetString(0));

                return results;
            }
            finally
            {
                await conn.CloseAsync();
            }
        }

        public void Dispose()
        {
            
        }
    }
}
