using System.Text.Json;
using ImageMagick;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using OllamaSharp.Models;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Consumers;

public class SummarisePhotosConsumer(
    IOllamaApiClient ollamaApiClient,
    PhotoSearchContext photoSearchContext,
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

        foreach (var filePath in context.Message.ImagePaths)
        {
            if (!photos.ContainsKey(filePath))
            {
                continue;
            }

            try
            {
                var summary = await GenerateCompletionRequest(context.Message.ModelName, filePath);
                photos[filePath].PhotoSummaries ??= [];
                photos[filePath].PhotoSummaries?.Add(summary);
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

    private PhotoSummary? ParseResponse(string jsonResponse, string modelName)
    {
        try
        {
            var document = JsonDocument.Parse(jsonResponse);
            var root = document.RootElement;
            if(!root.TryGetProperty("ImageSummary", out var imageSummary)||!root.TryGetProperty("ListOfObjects", out var objects) || 
               !root.TryGetProperty("CandidateCategories", out var imageCategories))
            {
                return null;
            }
            return new PhotoSummary
            {
                Description = imageSummary.GetString()!,
                Model = modelName,
                DateGenerated = DateTimeOffset.UtcNow,
                Categoties = imageCategories.EnumerateArray().Select(x => x.GetRawText()).ToList(),
                ObjectClasses = objects.EnumerateArray().Select(x => x.GetRawText()).ToList()
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error parsing ollama response for model {Model}. ", modelName);
            return null;
        }
    }
    private async Task<PhotoSummary?> GenerateCompletionRequest(string modelName, string filePath)
    {
        using var image = new MagickImage(filePath);
        var imageBytes = image.ToByteArray();
        var base64String = Convert.ToBase64String(imageBytes);

        var request = new GenerateCompletionRequest()
        {
            Prompt =
                "Given the attached photo, please provide a detailed summary and additional details as outlined below: "+
                "The response json should be as following: {\"ImageSummary\":\"\",\"ListOfObjects\":[\"\",\"\"],\"CandidateCategories\":[\"\",\"\"]}. " +
                "ImageSummary: Summarise the image in detail in this field. String field. "+
                "ListOfObjects: Identify object visible in the image. String array. "+
                "CandidateCategories: Provide a potential list of categories image belongs to. String array. ",
            Model = modelName,
            Stream = false,
            Context = [],
            Format = "json",
            Images = [base64String]
        };
        
        var result = await ollamaApiClient.GetCompletion(request);
        var photoSummary = ParseResponse(result.Response, modelName);
        if (photoSummary != null) return photoSummary;
        var retryCount = 0;
        while (photoSummary == null && retryCount < 4)
        {
            retryCount++;
            logger.LogWarning("Ollama returned invalid json. File name {File} model name {ModelName}. Retrying the Ollama API call. Retry attempt {Retry}", filePath, modelName, retryCount);
            request.Prompt = $"Ok let's try again. You have not returned a valid json. Please this time ensure it is a valid json for the same image. {request.Prompt}";
            request.Context = result.Context;
            result = await ollamaApiClient.GetCompletion(request);
            photoSummary = ParseResponse(result.Response, modelName);
        }
        if(photoSummary==null)
            logger.LogError("Ollama returned invalid json after {Retry} retries. File name {File} model name {ModelName}.", retryCount, filePath, modelName);
        
        return photoSummary;
    }

    private async Task<Dictionary<string, Photo>?> GetPhotosToUpdate(List<string> imagePaths)
    {
        Dictionary<string, Photo>? photos = null;
        try
        { 
            photos = await photoSearchContext.Photos.Where(
                photo => photo.PhotoSummaries == null &&  imagePaths.Contains(photo.ExactPath)).ToDictionaryAsync(x => x.ExactPath, x => x);
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting photos to summarise.");
        }

        return photos;
    }
}