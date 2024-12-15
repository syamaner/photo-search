using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ImageMagick;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class Florence2PhotoSummaryClient(HttpClient  httpClient) : IPhotoSummaryClient
{
    private static readonly string[] SupportedModels = ["Florence-2-large","Florence-2-large-ft","Florence-2-base-ft","Florence-2-base"];

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath, string base64Image, string address)
    {        
        var stopwatch = new Stopwatch();
        using var image = new MagickImage(imagePath);
        var imageBytes = image.ToByteArray();
        var base64String =  !string.IsNullOrWhiteSpace(base64Image) ? base64Image : Convert.ToBase64String(imageBytes);
        var requestBody = new Dictionary<string, string>
        {
            ["imagePath"] = imagePath,
            ["base64image"] = base64String
        };
        var requestPayload = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
        var result = await httpClient.PostAsync($"/api/summarise/{imagePath.Replace("/", "-")}", requestContent);

        var response = await result.Content.ReadFromJsonAsync<FlorenceResponse>();
        return new PhotoSummary()
        {
            Description = response?.Summary!,
            //Model = modelName,
            ObjectClasses = response!.Objects?.Distinct()?.ToList(),
            DateGenerated = DateTimeOffset.Now,
            PromptSummary =
                new PromptSummary(["todo"], modelName, stopwatch.Elapsed,
                    new Dictionary<string, object>()
                    {
                        ["todo"] = "todo"
                    })
        };
    }

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }
}