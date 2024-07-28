using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;

public class Geometry
{
    [JsonPropertyName("type")] public string Type { get; set; } = null!;

    [JsonPropertyName("coordinates")] public List<double> Coordinates { get; set; } = null!;
}