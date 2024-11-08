using System.Text;
using ImageMagick;
using OllamaSharp;
using OllamaSharp.Models;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class OllamaPhotoSummaryClient(IOllamaApiClient ollamaApiClient) : IPhotoSummaryClient
{
    private static readonly string[] SupportedModels =
        ["llava-phi3", "llava:7b", "llava:13b", "bakllava", "llava-llama3"];

    private const string PromptSummary = "Please provide a detailed description of the attached photo.";
    private const string PromptObjects = "Now please identify object visible in the image as a comma seperated list.";

    private const string PromptCategories =
        "Finally, provide a potential list of categories image belongs to as a commas seperated list.";

    private const int MaxRetryAttempts = 5;

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath)
    {
        using var image = new MagickImage(imagePath);
        var imageBytes = image.ToByteArray();
        var base64String = Convert.ToBase64String(imageBytes);

        var photoDescription = await RunPrompt(modelName, PromptSummary, [], base64String);
        // var photoDescription = result?.Response;

        string objects = await RunPrompt(modelName, PromptObjects, [], null);
        //var objects = result?.Response;

        string categories = await RunPrompt(modelName, PromptCategories, [], null);
        //  var categories = result?.Response;

        return new PhotoSummary()
        {
            Description = photoDescription!,
            Model = modelName,
            DateGenerated = DateTimeOffset.Now,
            ObjectClasses = objects?.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
            Categories = categories?.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
        };
    }

    private async Task<string> RunPrompt(string modelName, string prompt, long[]? context, string? base64Image)
    {
        var request = new GenerateRequest()
        {
            Prompt = prompt,
            Model = modelName,
            Stream = false,
            Context = context,
            Images = string.IsNullOrEmpty(base64Image) ? null : [base64Image]
        };

        var result = ollamaApiClient.GenerateAsync(request);
        var stbResult = new StringBuilder();

        await foreach (var stream in result)
        {
            stbResult.Append(stream?.Response);
        }

        return stbResult.ToString();
        /*while(string.IsNullOrEmpty(result?.Response) && attempt++ < MaxRetryAttempts)
        {
            result = await ollamaApiClient.GetCompletion(request);
        }

        return result;*/
    }
}