using MassTransit;
using PhotoSearch.Common;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;

namespace PhotoSearch.Worker.Consumers;

public class ImportPhotosConsumer(IPhotoImporter photoImporter, PhotoSearchContext photoSearchContext, ILogger<ImportPhotosConsumer> logger): IConsumer<ImportPhotos>
{
    public async Task Consume(ConsumeContext<ImportPhotos> context)
    {
        var photos = photoImporter.ImportPhotos(context.Message.Directory);

        await photoSearchContext.Photos.AddRangeAsync(photos);
        await photoSearchContext.SaveChangesAsync();
        
        logger.LogInformation("Inserted {COUNT} photos.",photos.Count);
    }
}