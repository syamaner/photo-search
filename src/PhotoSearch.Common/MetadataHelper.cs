using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoSearch.Common;

public class MetadataHelper
{
    private const string GpsLatitudeKey = "GPS-GPS Latitude";
    private const string GpsLatitudeRefKey = "GPS-GPS Latitude Ref";
    private const string GpsLongitudeKey = "GPS-GPS Longitude";
    private const string GpsLongitudeRefKey = "GPS-GPS Longitude Ref";

    public record GpsLocation(double Latitude, double Longitude);

    public static DateTime? GetImageCaptureTime(Dictionary<string, string> metadata)
    {
        var dateTimeOriginalStr = metadata.GetValueOrDefault("Exif SubIFD-Date/Time Original");
        var timeZoneOriginalStr = metadata.GetValueOrDefault("Exif SubIFD-Time Zone Original");

        if (string.IsNullOrWhiteSpace(dateTimeOriginalStr) ||
            string.IsNullOrWhiteSpace(timeZoneOriginalStr)) return null;
        // Parse the date and time
        var dateTimeOriginal = DateTime.ParseExact(dateTimeOriginalStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

        // Parse the time zone
        var timeZoneMatch = Regex.Match(timeZoneOriginalStr, @"([+-])(\d{2}):(\d{2})");
        if (!timeZoneMatch.Success) return null;
        
        var sign = timeZoneMatch.Groups[1].Value == "+" ? 1 : -1;
        var hours = int.Parse(timeZoneMatch.Groups[2].Value);
        var minutes = int.Parse(timeZoneMatch.Groups[3].Value);
        var offset = new TimeSpan(hours * sign, minutes, 0);

        // Combine the DateTime and TimeSpan to get DateTimeOffset
        var dateTimeOffset = new DateTimeOffset(dateTimeOriginal, offset);
        var utc = dateTimeOffset.ToUniversalTime();
        return dateTimeOffset.UtcDateTime;

    }
    public static GpsLocation? ConvertToDegrees(Dictionary<string, string> metadata)
    {
        if (metadata == null || metadata.Count == 0)
            throw new ArgumentException("Invalid argument. The metadata is empty.");

        var latitude = metadata.GetValueOrDefault(GpsLatitudeKey);
        var latitudeRef = metadata.GetValueOrDefault(GpsLatitudeRefKey);
        var longitude = metadata.GetValueOrDefault(GpsLongitudeKey);
        var longitudeRef = metadata.GetValueOrDefault(GpsLongitudeRefKey);
        if (string.IsNullOrEmpty(latitude))
            return null;
        var latitudeDegrees = ConvertToDegrees(latitude, latitudeRef);
        var longitudeDegrees = ConvertToDegrees(longitude, longitudeRef);

        return new GpsLocation(latitudeDegrees, longitudeDegrees);

    }
    private static double ConvertToDegrees(string? gpsCoordinate, string? reference)
    {
        if (string.IsNullOrWhiteSpace(gpsCoordinate) || string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Invalid argument. The GPS coordinate or reference is empty.");

        // Splitting the coordinate into degrees, minutes, and seconds
        var dms = gpsCoordinate.Split(new[] { '°', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries);
        if (dms.Length != 3)
            throw new ArgumentException("Invalid GPS coordinate format.");

        // Parsing degrees, minutes, and seconds
        var degrees = double.Parse(dms[0], CultureInfo.InvariantCulture);
        var minutes = double.Parse(dms[1], CultureInfo.InvariantCulture);
        var seconds = double.Parse(dms[2], CultureInfo.InvariantCulture);

        // Calculating decimal degrees
        var result = degrees + (minutes / 60) + (seconds / 3600);

        // Adjusting sign based on reference
        return reference is "S" or "W" ? -result : result;
    }
}