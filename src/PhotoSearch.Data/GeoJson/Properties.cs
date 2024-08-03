using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;

public class Properties
{
    [JsonPropertyName("place_id")] public int PlaceId { get; set; }

    [JsonPropertyName("osm_type")] public string OsmType { get; set; } = null!;

    [JsonPropertyName("osm_id")] public long OsmId { get; set; }

    [JsonPropertyName("place_rank")] public int PlaceRank { get; set; }

    [JsonPropertyName("category")] public string Category { get; set; } = null!;

    [JsonPropertyName("type")] public string Type { get; set; } = null!;

    [JsonPropertyName("importance")] public double Importance { get; set; }

    [JsonPropertyName("addresstype")] public string Addresstype { get; set; } = null!;

    [JsonPropertyName("name")] public string Name { get; set; } = null!;

    [JsonPropertyName("display_name")] public string DisplayName { get; set; } = null!;

    [JsonPropertyName("address")] public Address Address { get; set; } = null!;

    [JsonPropertyName("namedetails")] public Namedetails Namedetails { get; set; } = null!;
}