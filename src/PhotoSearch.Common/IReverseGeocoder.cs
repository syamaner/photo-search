using PhotoSearch.Data.GeoJson;

namespace PhotoSearch.Common;

public interface IReverseGeocoder
{
    Task<FeatureCollection?> ReverseGeocode(double latitude, double longitude,CancellationToken cancellationToken);
}