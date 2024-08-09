namespace PhotoSearch.API.Endpoints.PhotoRetrieval;

public class Models
{
    public record GetPhotosRequest();
    public record GetPhotosResponse(string Id, Dictionary<string, string> Summaries, double? Latitude, double? Longitude, string Address);
    
}