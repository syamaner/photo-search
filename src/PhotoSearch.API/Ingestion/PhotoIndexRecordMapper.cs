using Microsoft.Extensions.VectorData;
using PhotoSearch.API.Models;
using Qdrant.Client.Grpc;

namespace PhotoSearch.API.Ingestion;

[Obsolete("Obsolete")]
public class PhotoIndexRecordMapper : IVectorStoreRecordMapper<PhotoIndexRecord, PointStruct>
{
    
    public PointStruct MapFromDataToStorageModel(PhotoIndexRecord dataModel)
    {
        var pointStruct = new PointStruct
        {
            Id = new PointId { Uuid = dataModel.Id.ToString() },
            Vectors = new Vectors(),
            Payload =
            {
                { Constants.VectorFieldNames.ContentName, dataModel.Content },
                { Constants.VectorFieldNames.FilePathName, dataModel.FilePath },
                { Constants.VectorFieldNames.CaptureDateName, (double)dataModel.CaptureDate.ToUnixTimeSeconds() },
                { Constants.VectorFieldNames.VectorFieldName, new Value { StructValue = new Struct() } }
            },
        };

        if (dataModel.Vector == null) return pointStruct;
        
        var namedVectors = new NamedVectors();
        namedVectors.Vectors.Add(Constants.VectorFieldNames.VectorFieldName,
            dataModel.Vector.Value.ToArray());
        pointStruct.Vectors.Vectors_ = namedVectors;

        return pointStruct;
    }

    public PhotoIndexRecord MapFromStorageToDataModel(PointStruct storageModel, StorageToDataModelMapperOptions options)
    {
        var faqRecord = new PhotoIndexRecord
        {
            Id = Guid.Parse(storageModel.Id.Uuid),
            Content = storageModel.Payload[Constants.VectorFieldNames.ContentName].StringValue,
            FilePath = storageModel.Payload[Constants.VectorFieldNames.FilePathName].StringValue,
            CaptureDate = DateTimeOffset.FromUnixTimeSeconds((long)
                storageModel.Payload[Constants.VectorFieldNames.CaptureDateName].DoubleValue),
            Vector = storageModel.Vectors != null
                ? new ReadOnlyMemory<float>(storageModel.Vectors.Vectors_.Vectors[Constants.VectorFieldNames.VectorFieldName].Data
                    .ToArray())
                : null
        };

        return faqRecord;
    }
}