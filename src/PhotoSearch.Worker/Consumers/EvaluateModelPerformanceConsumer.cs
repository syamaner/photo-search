using MassTransit;
using MongoDB.Driver;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;
using PhotoSearch.Worker.Clients;

namespace PhotoSearch.Worker.Consumers;

public class EvaluateModelPerformanceConsumer(
    IPhotoSummaryEvaluator photoSummaryEvaluator,
    IMongoCollection<Photo> collection,
    ILogger<EvaluateModelPerformanceConsumer> logger) : IConsumer<EvaluateModelSummaries>
{
    public async Task Consume(ConsumeContext<EvaluateModelSummaries> context)
    {
        var photos = collection.AsQueryable().ToList();

        foreach (var photo in photos)
        {
            foreach (var photoSummary in photo.PhotoSummaries!)
            {
                try
                {
                    photoSummary.Value.PhotoSummaryScore = await photoSummaryEvaluator.EvaluatePhotoSummary(
                        photo.Base64Data,
                        new ImageSummaryEvaluationRequest(photoSummary.Value.Description,
                            photoSummary.Value.ObjectClasses!,
                            photoSummary.Value.Categories!));

                    await collection.ReplaceOneAsync(doc => doc.RelativePath == photo.RelativePath, photo);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error evaluating photo summary for {PHOTO}", photo.RelativePath);
                }
            }
        }
        
        logger.LogInformation("Inserted {COUNT} photos.", photos.Count);
    }
}