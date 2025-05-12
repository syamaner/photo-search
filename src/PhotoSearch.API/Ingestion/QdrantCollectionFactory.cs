using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using PhotoSearch.API.Models;
using Qdrant.Client;

#pragma warning disable CS8603 // Possible null reference return.

namespace PhotoSearch.API.Ingestion;

public class QdrantCollectionFactory(string embeddingModel="nomic-embed-text") : IQdrantVectorStoreRecordCollectionFactory
{
    private static readonly Dictionary<string, int> EmbeddingModels = new()
    {
        { "mxbai-embed-large", 1024 },
        { "nomic-embed-text", 768 },
        { "granite-embedding:30m", 384 },
        { "text-embedding-3-large", 3072 },
        { "snowflake-arctic-embed", 1024 }
    };


    [Obsolete("Obsolete")] private readonly VectorStoreRecordDefinition _faqRecordDefinition = new()
    {
        Properties = new List<VectorStoreRecordProperty>
        {
            new VectorStoreRecordKeyProperty("Id", typeof(Guid)),
            new VectorStoreRecordDataProperty("Content",
                typeof(string)) { IsFilterable = true, StoragePropertyName = Constants.VectorFieldNames.ContentName },
            new VectorStoreRecordDataProperty("FilePath",
                typeof(string)) { IsFilterable = true, StoragePropertyName = Constants.VectorFieldNames.FilePathName },
            new VectorStoreRecordDataProperty("CaptureDate",
                typeof(double)) { IsFilterable = true, StoragePropertyName = Constants.VectorFieldNames.CaptureDateName },
            
            new VectorStoreRecordVectorProperty("Vector", typeof(float))
            {
                Dimensions = EmbeddingModels.ContainsKey(embeddingModel) ? EmbeddingModels[embeddingModel] : 384,
                DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw,
                StoragePropertyName = Constants.VectorFieldNames.VectorFieldName
            },
        }
    };
    
    public IVectorStoreRecordCollection<TKey, TRecord> CreateVectorStoreRecordCollection<TKey, TRecord>(
        QdrantClient qdrantClient, string name, VectorStoreRecordDefinition? vectorStoreRecordDefinition)
        where TKey : notnull
    {

        if ( typeof(TRecord) == typeof(PhotoIndexRecord))
        {
            var customCollection = new QdrantVectorStoreRecordCollection<PhotoIndexRecord>(
                qdrantClient,
                name,
                new QdrantVectorStoreRecordCollectionOptions<PhotoIndexRecord>
                {
                    HasNamedVectors = true,
                    PointStructCustomMapper = new PhotoIndexRecordMapper(),
                    VectorStoreRecordDefinition = _faqRecordDefinition //vectorStoreRecordDefinition
                }) as IVectorStoreRecordCollection<TKey, TRecord>;
            return customCollection;
        }

        // Otherwise, just create a standard collection with the default mapper.
        var collection = new QdrantVectorStoreRecordCollection<TRecord>(
            qdrantClient,
            name,
            new QdrantVectorStoreRecordCollectionOptions<TRecord>
            {
                VectorStoreRecordDefinition = vectorStoreRecordDefinition
            }) as IVectorStoreRecordCollection<TKey, TRecord>;
        return collection;
    }
}