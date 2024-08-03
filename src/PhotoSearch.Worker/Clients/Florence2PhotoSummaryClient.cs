using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ImageMagick;
using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public class Florence2PhotoSummaryClient(HttpClient  httpClient) : IPhotoSummaryClient
{
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
        try
        {
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var result = await httpClient.PostAsync($"/api/summarise/{imagePath.Replace("/", "-")}", content);

            var response = await result.Content.ReadFromJsonAsync<FlorenceResponse>();
            return new PhotoSummary()
            {
                Description = response?.Summary!,
                Model = modelName,
                ObjectClasses = response!.Objects?.Distinct()?.ToList(),
                DateGenerated = DateTimeOffset.Now
            };
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public bool CanHandle(string modelName)
    {
        return SupportedModels.Contains(modelName, StringComparer.OrdinalIgnoreCase);
    }
}