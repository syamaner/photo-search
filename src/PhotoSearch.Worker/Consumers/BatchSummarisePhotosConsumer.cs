using System.Diagnostics;
using MassTransit;
using MongoDB.Driver;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;
using PhotoSearch.ServiceDefaults;
using PhotoSearch.Worker.Clients;

namespace PhotoSearch.Worker.Consumers;

// ReSharper disable once ClassNeverInstantiated.Global
public class BatchSummarisePhotosConsumer(
    IMongoCollection<Photo> collection, ConsoleMetrics metrics,
    IEnumerable<IPhotoSummaryClient> photoSummaryClients,
    ILogger<ImportPhotosConsumer> logger) : IConsumer<BatchSummarisePhotos>
{   

    public async Task Consume(ConsumeContext<BatchSummarisePhotos> context)
    {
        using var activity =  TracingConstants.WorkerActivitySource.StartActivity("Handle BatchSummarisePhotos");
        
        var photos = await GetPhotosToUpdate();
        if (photos == null || photos.Count == 0)
        {
            return;
        }

        foreach (var modelName in context.Message.ModelNames)
        {
            activity.AddTag("Model", modelName);
            logger.LogInformation("Summarising {COUNT} photos using {Model}.", photos.Count, modelName);

            var updateCount = 0;
            foreach (var photoPath in photos.Select(x => x.ExactPath))
            {
                
                using var summaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Summarise Photo");
                summaryActivity.AddTag("Photo", photoPath);
                try
                {
                    var photo = photos.SingleOrDefault(x => x.ExactPath == photoPath);
                    if (photo == null)
                        continue;
                    if(photo.PhotoSummaries!=null && photo.PhotoSummaries.ContainsKey(modelName)&& !string.IsNullOrWhiteSpace(photo.PhotoSummaries[modelName].Description))
                    {
                        continue;
                    }
                    var address = photo.LocationInformation?.Features?.Select(x => x.Properties.DisplayName).FirstOrDefault();
                    var summary = await SummarisePhoto(modelName, photoPath, photo.Base64Data,address);
                    photo!.PhotoSummaries ??= new Dictionary<string, PhotoSummary>();
                    if (photo?.PhotoSummaries.ContainsKey(modelName) ?? false)
                    {
                        photo.PhotoSummaries.Remove(modelName);
                    }

                    photo!.PhotoSummaries!.Add(modelName, summary!);
                    var replaceOneResult = await collection.ReplaceOneAsync(
                        doc => doc.RelativePath == photo.RelativePath,
                        photo);

                    updateCount += (int)replaceOneResult.MatchedCount;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing ollama response. File name {File}", photoPath);
                }
            }

            logger.LogInformation("Summarised {COUNT} photos using {Model}.", updateCount, modelName);
        }
    }

    private async Task<PhotoSummary?> SummarisePhoto(string modelName, string filePath, string base64Image, string address)
    {
      //  modelName = "gpt-4o-mini";
        Stopwatch stopwatch = new();
        stopwatch.Start();
        using var summaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Call photoSummaryClients");
        var client = photoSummaryClients.FirstOrDefault(x => x.CanHandle(modelName));
        if (client == null)
        {
            logger.LogError("There is no summary client registered for model {Model}", modelName);
            return null;
        }

        var results = await client!.SummarisePhoto(modelName, filePath, base64Image,address);
        stopwatch.Stop();
        metrics.PhotoSummarised(modelName, 1);
        metrics.PhotoSummaryTiming(modelName, filePath, stopwatch.Elapsed.TotalSeconds);
        return results;
    }

    private async Task<List<Photo>?> GetPhotosToUpdate()
    {        
        using var dbActivity = TracingConstants.WorkerActivitySource.StartActivity("Get Photos to Update");
        return await collection.AsQueryable().ToListAsync();
    }
}