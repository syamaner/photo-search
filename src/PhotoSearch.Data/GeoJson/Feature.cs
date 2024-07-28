using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;

public class Feature
{
    [JsonPropertyName("type")] public string Type { get; set; } = null!;

    [JsonPropertyName("properties")] public Properties Properties { get; set; }= null!;

    [JsonPropertyName("bbox")] public List<double> Bbox { get; set; } = null!;

    [JsonPropertyName("geometry")] public Geometry Geometry { get; set; } = null!;
}