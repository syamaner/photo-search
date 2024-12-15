using MassTransit;
using MongoDB.Driver;
using PhotoSearch.Common;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Consumers;

public class ImportPhotosConsumer(IPhotoImporter photoImporter, IMongoCollection<Photo> collection, IBus bus, ILogger<ImportPhotosConsumer> logger): IConsumer<ImportPhotos>
{
    public async Task Consume(ConsumeContext<ImportPhotos> context)
    {
        var existingIds = collection.AsQueryable().Select(x => x.RelativePath).ToList();
        
        var photos = await photoImporter.ImportPhotos(context.Message.Directory,existingIds);

        if (photos.Count != 0)
        {
            await collection.InsertManyAsync(photos);
            await bus.Publish<PhotoSummary>(new { ImagePaths = photos.Select(p => p.ExactPath).ToArray() });
        }

        logger.LogInformation("Inserted {COUNT} photos.",photos.Count);
    }
}