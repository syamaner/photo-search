namespace PhotoSearch.API.Endpoints.PhotoRetrieval;

public class Models
{
    public record GetImageRequest(string ImageId, int MaxWidth, int MaxHeight);
    public record GetPhotosResponse(string Id, Dictionary<string, ModelResponse> Summaries, double? Latitude, double? Longitude, string Address, string Base64Image);

    public record ModelResponse(string Summary, List<string>? Categories, List<string>? DetectedObjects);
}