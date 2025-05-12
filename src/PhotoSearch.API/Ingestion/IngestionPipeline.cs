using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using PhotoSearch.API.Models;

#pragma warning disable SKEXP0001

namespace PhotoSearch.API.Ingestion;

public class IngestionPipeline(
    Kernel kernel,
    ILogger<IngestionPipeline> logger,
    IChunker documentChunker)
{
    public async Task IngestDataAsync(string filePath, string embeddingModel)
    {
        var vectorStore = kernel.GetRequiredService<IVectorStore>(embeddingModel);
        var faqCollection = vectorStore.GetCollection<Guid, PhotoIndexRecord>(embeddingModel);
        var embeddingGenerator =
            kernel.GetRequiredService<ITextEmbeddingGenerationService>(embeddingModel);
        await EnsureCollectionExists(faqCollection, true);

        var documentsProcessed = 0; 
 

        await foreach (var fileChunk in documentChunker.GetChunks(filePath))
        {
            try
            {
                IList<ReadOnlyMemory<float>>? embeddings = null;

                embeddings = await embeddingGenerator.GenerateEmbeddingsAsync(fileChunk.Chunks);


                for (var i = 0; i < fileChunk.Chunks.Count; i++)
                {
                    try
                    {
                        var faqRecord = new PhotoIndexRecord()
                        {
                            Id = Guid.NewGuid(),
                            Content = fileChunk.Chunks[i],
                            Vector = embeddings[i],
                            CaptureDate = DateTimeOffset.MaxValue,
                            FilePath = ""
                        };
                        await faqCollection.UpsertAsync(faqRecord);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error inserting the vectors into vector store.");
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error generating embeddings for the file {FILENAME}", fileChunk.FileName);
            }

            documentsProcessed++;
        }

    }

    private static async Task EnsureCollectionExists(IVectorStoreRecordCollection<Guid, PhotoIndexRecord> faqCollection,
        bool forceRecreate = false)
    {
        var collectionExists = await faqCollection.CollectionExistsAsync();
        switch (collectionExists)
        {
            case true when !forceRecreate:
                return;
            case true:
                await faqCollection.DeleteCollectionAsync();
                break;
        }

        await faqCollection.CreateCollectionAsync();
    }
}