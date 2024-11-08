using System.Text.Json.Serialization;

namespace PhotoSearch.Worker.Clients;

public class FlorenceResponse
{
    [JsonPropertyName("objects")]
    public List<string> Objects { get; set; } = null!;
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = null!;
}