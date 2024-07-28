using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;


public class FeatureCollection
{
    [JsonPropertyName("type")] public string Type { get; set; } = null!;

    [JsonPropertyName("licence")] public string Licence { get; set; } = null!;

    [JsonPropertyName("features")] public List<Feature> Features { get; set; } = null!;
}