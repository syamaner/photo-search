using MassTransit;
using Microsoft.EntityFrameworkCore;
using PhotoSearch.Common;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Consumers;

public class ImportPhotosConsumer(IPhotoImporter photoImporter, PhotoSearchContext photoSearchContext, IBus bus, ILogger<ImportPhotosConsumer> logger): IConsumer<ImportPhotos>
{
    public async Task Consume(ConsumeContext<ImportPhotos> context)
    {
        var existingIds = await photoSearchContext.Photos.Select(x => x.RelativePath).ToListAsync();
        var photos = await photoImporter.ImportPhotos(context.Message.Directory,existingIds);
    
        await photoSearchContext.Photos.AddRangeAsync(photos);
        await photoSearchContext.SaveChangesAsync();

        await bus.Publish<PhotoSummary>(new { ImagePaths = photos.Select(p => p.ExactPath).ToArray() });
        logger.LogInformation("Inserted {COUNT} photos.",photos.Count);
    }
}