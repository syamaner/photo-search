using System.Text.Json.Serialization;

namespace PhotoSearch.Data.GeoJson;

public class Address
{
    [JsonPropertyName("building")] public string Building { get; set; } = null!;

    [JsonPropertyName("house_number")] public string HouseNumber { get; set; } = null!;

    [JsonPropertyName("road")] public string Road { get; set; } = null!;

    [JsonPropertyName("town")] public string Town { get; set; } = null!;

    [JsonPropertyName("city")] public string City { get; set; } = null!;

    [JsonPropertyName("county")] public string County { get; set; } = null!;

    [JsonPropertyName("state")] public string State { get; set; } = null!;

    [JsonPropertyName("ISO3166-2-lvl4")] public string Iso31662Lvl4 { get; set; } = null!;

    [JsonPropertyName("postcode")] public string Postcode { get; set; } = null!;

    [JsonPropertyName("country")] public string Country { get; set; } = null!;

    [JsonPropertyName("country_code")] public string CountryCode { get; set; } = null!;
}