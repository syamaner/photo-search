using MassTransit;
using MongoDB.Driver;
using OllamaSharp;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;
using PhotoSearch.Worker.Clients;

namespace PhotoSearch.Worker.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class SummarisePhotosConsumer(
    IMongoCollection<Photo> collection, 
    IEnumerable<IPhotoSummaryClient> photoSummaryClients,
    ILogger<ImportPhotosConsumer> logger) : IConsumer<SummarisePhotos>
{
    public async Task Consume(ConsumeContext<SummarisePhotos> context)
    {
        var photos = await GetPhotosToUpdate(context.Message.ImagePaths);
        var updateCount = 0;
        if (photos == null || photos.Count == 0)
        {
            return;
        }
        logger.LogInformation("Summarising {COUNT} photos using {Model}.",photos.Count,
            context.Message.ModelName);
        foreach (var filePath in context.Message.ImagePaths.Where(filePath => photos.Any(p => p.ExactPath == filePath)))
        {
            try
            {
                var photo = photos.SingleOrDefault(x=>x.ExactPath==filePath);
                if(photo==null)
                    continue;
                
                var address = photo.LocationInformation?.Features?.Select(x => x.Properties.DisplayName).FirstOrDefault();
                var summary = await SummarisePhoto(context.Message.ModelName, filePath, address);
                photo!.PhotoSummaries ??=  new Dictionary<string, PhotoSummary>();
                if (photo?.PhotoSummaries.ContainsKey(context.Message.ModelName)??false)
                {
                    photo.PhotoSummaries.Remove(context.Message.ModelName);
                }
       
                photo!.PhotoSummaries!.Add(context.Message.ModelName, summary!);
                var replaceOneResult = await collection.ReplaceOneAsync(
                    doc => doc.RelativePath ==photo.RelativePath, 
                    photo);

                updateCount += (int)replaceOneResult.MatchedCount;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing ollama response. File name {File}", filePath);
            }
        }

        
        logger.LogInformation("Summarised {COUNT} photos using {Model}.",updateCount,
            context.Message.ModelName);
    }

    private async Task<PhotoSummary?> SummarisePhoto(string modelName, string filePath, string address)
    {
        var client = photoSummaryClients.FirstOrDefault(x => x.CanHandle(modelName));
        if (client == null)
        {
            logger.LogError("There is no summary client registered for model {Model}",modelName);
        }
        
        return await client!.SummarisePhoto(modelName, filePath, address);
    }
    
    private async Task<List<Photo>?> GetPhotosToUpdate(List<string> imagePaths)
    {
        List<Photo>? photos = null;
        try
        {
            photos = await collection.AsQueryable().ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting photos to summarise.");
        }

        return photos;
    }
}