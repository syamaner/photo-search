using System.Net.Http.Json;
using PhotoSearch.Data.GeoJson;

namespace PhotoSearch.Common;

public class NominatimReverseGeocoder(HttpClient client) : IReverseGeocoder
{
    public async Task<FeatureCollection?> ReverseGeocode(double latitude, double longitude, CancellationToken cancellationToken)
    {
        string result2 = await client.GetStringAsync($"reverse?format=geojson&namedetails=1&lat={latitude}&lon={longitude}", cancellationToken);
        var result = await client.GetFromJsonAsync<FeatureCollection>($"reverse?format=geojson&namedetails=1&lat={latitude}&lon={longitude}", cancellationToken);
        return result;
    }
}