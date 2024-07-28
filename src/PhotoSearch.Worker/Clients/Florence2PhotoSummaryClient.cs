using System.Text.Json;
using ImageMagick;
using PhotoSearch.Data.Models;
using RestSharp;

namespace PhotoSearch.Worker.Clients;

public class Florence2PhotoSummaryClient : IPhotoSummaryClient
{
    private static readonly string? FlorenceUrl = Environment.GetEnvironmentVariable("services__florence2api__http__0");
    private readonly RestClientOptions _options = new($"{FlorenceUrl}/api");
    private static readonly string[] SupportedModels = ["Florence-2-large","Florence-2-large-ft","Florence-2-base-ft","Florence-2-base"];

    public async Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath)
    {
        using var image = new MagickImage(imagePath);
        var imageBytes = image.ToByteArray();
        var base64Image = Convert.ToBase64String(imageBytes);
        var requestBody = new Dictionary<string, string>
        {
            ["imagePath"] = imagePath,
            ["base64image"] = base64Image
        };
        var payload = JsonSerializer.Serialize(requestBody);
        
        using var client = new RestClient(_options);
        var request = new RestRequest($"summarise/{imagePath.Replace("/", "-")}", Method.Post).AddJsonBody(payload);
        var response = await client.PostAsync<FlorenceResponse>(request);
        
        return new PhotoSummary()
        {
            Description = response?.Summary!,
            Model = modelName,
            ObjectClasses = response!.Objects?.Distinct()?.ToList(),
            DateGenerated = DateTimeOffset.Now
        };

    }

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }
}