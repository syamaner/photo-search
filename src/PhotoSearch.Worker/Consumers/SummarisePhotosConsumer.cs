using MassTransit;
using MongoDB.Driver;
using OllamaSharp;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;
using PhotoSearch.Worker.Clients;

namespace PhotoSearch.Worker.Consumers;

public class SummarisePhotosConsumer(
    IOllamaApiClient ollamaApiClient,
    IMongoCollection<Photo> collection, 
    IEnumerable<IPhotoSummaryClient> photoSummaryClients,
    ILogger<ImportPhotosConsumer> logger) : IConsumer<SummarisePhotos>
{
    public async Task Consume(ConsumeContext<SummarisePhotos> context)
    {
        var photos = await GetPhotosToUpdate(context.Message.ImagePaths);
        ollamaApiClient.SelectedModel = context.Message.ModelName;
        int updateCount = 0;
        if (photos == null || photos.Count == 0)
        {
            return;
        }
        logger.LogInformation("Summarising {COUNT} photos using {Model}.",photos.Count,
            context.Message.ModelName);
        foreach (var filePath in context.Message.ImagePaths)
        {
            if (photos.All(p => p.ExactPath != filePath))
            {
                continue;
            }

            try
            {
                var photo = photos.SingleOrDefault(x=>x.ExactPath==filePath);
                if(photo==null)
                    continue;
                var summary = await SummarisePhoto(context.Message.ModelName, filePath);
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

    private async Task<PhotoSummary?> SummarisePhoto(string modelName, string filePath)
    {
        var client = photoSummaryClients.FirstOrDefault(x => x.CanHandle(modelName));
        if (client == null)
        {
            logger.LogError("There is no summary client registered for model {Model}",modelName);
        }
        
        return await client!.SummarisePhoto(modelName, filePath);
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