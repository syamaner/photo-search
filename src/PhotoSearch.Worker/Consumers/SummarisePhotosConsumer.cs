using System.Text.Json;
using System.Text.RegularExpressions;
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
        int updateCOunt = 0;
        var photos = await GetPhotosToUpdate(context.Message.ImagePaths);
        ollamaApiClient.SelectedModel = context.Message.ModelName;

        if (photos == null || !photos.Any())
        {
            return;
        }

        foreach (var filePath in context.Message.ImagePaths)
        {
            if (!photos.ContainsKey(filePath))
            {
                continue;
            }

            var request = BuildRequest(context.Message.ModelName, filePath);
            var result = await ollamaApiClient.GetCompletion(request);
            var response = result.Response.Replace(Environment.NewLine, "");
            var pattern = @"\\u([0-9A-Fa-f]{4})";
            response = Regex.Replace(response, pattern, match =>
            {
                char unicodeChar = (char)Convert.ToInt32(match.Groups[1].Value, 16);
                return unicodeChar.ToString();
            });
            var jsonMatch = Regex.Match(response, @"\{.*\}", RegexOptions.Singleline);
            if (!jsonMatch.Success) continue;
            
            var jsonString = jsonMatch.Value;
            try
            {
                var document = JsonDocument.Parse(jsonString);
                var root = document.RootElement;
                var imageSummary = root.GetProperty("ImageSummary").GetRawText();
                var objects = root.GetProperty("Objects").EnumerateArray()
                    .Select(x => x.GetRawText()?.Replace("\"", "")).ToList();
                var imageCategories = root.GetProperty("ImageCategories").EnumerateArray()
                    .Select(x => x.GetRawText()?.Replace("\"", "")).ToList();
                photos[filePath].PhotoSummaries ??= [];
                photos[filePath].PhotoSummaries?.Add(new PhotoSummary
                {
                    Description = imageSummary,
                    Model = context.Message.ModelName,
                    DateGenerated = DateTimeOffset.UtcNow,
                    Categoties = imageCategories,
                    ObjectClasses = objects
                });
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

    private static GenerateCompletionRequest BuildRequest(string modelName, string filePath)
    {
        using var image = new MagickImage(filePath);
        byte[] imageBytes = image.ToByteArray();
        var base64String = Convert.ToBase64String(imageBytes);

        var request = new GenerateCompletionRequest()
        {
            Prompt =
                "Please provide the response as valid json as I will need to parse it in code. "+
                "There should be 3 properties in the response as following. "+
                "ImageSummary: describe the image in detail in this field. String field. "+
                "Objects: list of strings. Identify object classes visible in the image. This must be always a json array. "+
                "ImageCategories: list of strings. Provide a possible isst of categories image belongs to. This also has to be a valid json array. "+
                "Can you summarise this photo?",
            Model = modelName,
            Stream = true,
            Context = [],
            Images = [base64String]
        };
        return request;
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
            
        }

        return photos;
    }
}