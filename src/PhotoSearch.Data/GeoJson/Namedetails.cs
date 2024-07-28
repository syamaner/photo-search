using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;

public class Namedetails
{
    [JsonPropertyName("name")] public string Name { get; set; } = null!;
}