using System.Diagnostics;
using System.Text;
using ImageMagick;
using MassTransit.Internals;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using PhotoSearch.Data.Models;
using PhotoSearch.ServiceDefaults;

namespace PhotoSearch.Worker.Clients;

public class OllamaPhotoSummaryClient(IOllamaApiClient ollamaApiClient) : IPhotoSummaryClient
{
    private static readonly string[] SupportedModels =
        ["llava-phi3", "llava:7b", "llava:13b", "bakllava", "llava-llama3", "llama3.2-vision"];

    private const string SystemPrompt = """
                                        You are an expert image analyst tasked with providing detailed and accurate summaries of photos. For every photo, follow these steps:
                                        Describe the photo in detail. This should be a coherent and detailed description of the photo. 
                                        Don't use headers in the response. Use naturally flowing sentences.
                                        The user prompts will sometimes include address information to help you provide a more accurate description. You can use this information to verify the content of the photo.
                                        Formatting:
                                        Use clear, concise sentences.
                                        Always ensure your descriptions are objective and precise, avoiding any assumptions beyond the visible details.
                                        """;
    private const string PromptSummary = "Please provide a detailed description of the attached photo. Please not that the location is as following : {0}";
    private const string PromptObjects = "Now please identify most relevant top 10 object visible in the photo as a comma seperated list.";

    private const string PromptCategories = "Please, provide a list of most relevant top 10 categories the photo belongs to as a commas seperated list.";

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath, string address)
    {
        using var image = new MagickImage(imagePath);
        var imageBytes = image.ToByteArray();
        var base64String = Convert.ToBase64String(imageBytes);
        var stopwath = new Stopwatch();
        stopwath.Start();
        var chat = new Chat(ollamaApiClient,SystemPrompt)
        {
            Model = modelName,
            Options = new RequestOptions()
            {
                Temperature = 0.001f
            }
        };
  
        using var summaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Generate Description");
        var photoDescription = await RunPrompt1(chat, string.Format(PromptSummary,address), base64String);
        summaryActivity?.Dispose();
        
        using var objectSummaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Detect Objects");
        var objects = await RunPrompt1(chat, PromptObjects, null);
        objectSummaryActivity?.Dispose();
        
        using var categorySummaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Detect Categories");
        var categories = await RunPrompt1(chat, PromptCategories, null);
        categorySummaryActivity?.Dispose();
            
        stopwath.Stop();
        return new PhotoSummary()
        {
            Description = photoDescription,
            DateGenerated = DateTimeOffset.Now,
            ObjectClasses = objects.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
            Categories = categories.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
            PromptSummary =
                new PromptSummary([PromptSummary, PromptObjects, PromptCategories, SystemPrompt], modelName, stopwath.Elapsed,
                    new Dictionary<string, object>()
                    {
                        [nameof(chat.Options.Temperature)] = chat.Options.Temperature
                    })
        };
    }
    private async Task<string> RunPrompt1(Chat chat,string prompt, string? base64Image)
    {
        using var categorySummaryActivity = TracingConstants.WorkerActivitySource.StartActivity("Send Chat");
        var result = chat.SendAsync(prompt, string.IsNullOrWhiteSpace(base64Image) ? null:  [base64Image]);
        var stbResult = new StringBuilder();
        var results = await result.ToListAsync();
          foreach (var res in results)
        {
            stbResult.Append(res);
        }  

        return stbResult.ToString();
    }

}