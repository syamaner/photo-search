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
                var summary = await SummarisePhoto(context.Message.ModelName, filePath);
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
    
    private async Task<PhotoSummary?> SummarisePhoto(string modelName, string filePath)
    {
        using var image = new MagickImage(filePath);
        var imageBytes = image.ToByteArray();
        var base64String = Convert.ToBase64String(imageBytes);

        var promptSummary = "Please provide a detailed description of the attached photo.";
        var promptObjects = "Now please identify object visible in the image as a comma seperated list.";
        var promptCategories = "Finally, provide a potential list of categories image belongs to as a commas seperated list."; 
        
        var request = new GenerateCompletionRequest()
        {
            Prompt =promptSummary,
            Model = modelName,
            Stream = false,
            Context = [],
            //Format = "json",
            Images = [base64String]
        };
        
        var result = await ollamaApiClient.GetCompletion(request);
        
        var photoSummary = result.Response;
        
        request.Prompt = promptObjects;
        request.Context = result.Context;
        request.Images = null;
        result = await ollamaApiClient.GetCompletion(request);
        var objects = result.Response;
        
        request.Prompt = promptCategories;
        request.Context = result.Context;
        result = await ollamaApiClient.GetCompletion(request);
        var categories = result.Response;
        
        return new PhotoSummary()
        {
             Description = photoSummary,
             Model = modelName,
             DateGenerated = DateTimeOffset.Now,
             ObjectClasses = objects?.Split(",",StringSplitOptions.RemoveEmptyEntries).ToList(),
             Categoties = categories?.Split(",",StringSplitOptions.RemoveEmptyEntries).ToList()
        };
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