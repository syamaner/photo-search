using MassTransit;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;
using PhotoSearch.Data.Models;
using PhotoSearch.Worker.Clients;

namespace PhotoSearch.Worker.Consumers;

public class SummarisePhotosConsumer(
    IOllamaApiClient ollamaApiClient,
    PhotoSearchContext photoSearchContext, 
    IEnumerable<IPhotoSummaryClient> photoSummaryClients,
    ILogger<ImportPhotosConsumer> logger) : IConsumer<SummarisePhotos>
{
    public async Task Consume(ConsumeContext<SummarisePhotos> context)
    {
        var photos = await GetPhotosToUpdate(context.Message.ImagePaths);
        ollamaApiClient.SelectedModel = context.Message.ModelName;

        if (photos == null || photos.Count == 0)
        {
            return;
        }
        logger.LogInformation("Summarising {COUNT} photos using {Model}.",photos.Count,
            context.Message.ModelName);
        foreach (var filePath in context.Message.ImagePaths)
        {
            if (!photos.ContainsKey(filePath))
            {
                continue;
            }

            try
            {
                var summary = await SummarisePhoto(context.Message.ModelName, filePath);
                photos[filePath].PhotoSummaries ??= new Dictionary<string, PhotoSummary>();
                photos[filePath].PhotoSummaries![context.Message.ModelName] = summary;
                photoSearchContext.Update(photos[filePath]);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error parsing ollama response. File name {File}", filePath);
            }
        }

        var updateCount = await photoSearchContext.SaveChangesAsync();
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
    
    private async Task<Dictionary<string, Photo>?> GetPhotosToUpdate(List<string> imagePaths)
    {
        Dictionary<string, Photo>? photos = null;
        try
        {
            photos = await photoSearchContext.Photos.ToDictionaryAsync(x => x.ExactPath, x => x);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting photos to summarise.");
        }

        return photos;
    }
}