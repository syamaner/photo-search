using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PhotoSearch.Data.GeoJson;

namespace PhotoSearch.Data.Models;

public class Photo
{
    [MaxLength(250)]
    public required string RelativePath { get; init; }
    
    [MaxLength(600)]
    public required string ExactPath { get; init; }
    
    [MaxLength(500)]
    public string? PublicUrl { get; init; }
    
    [MaxLength(30)]
    public required string FileType { get; init; }
    public DateTime? CaptureDateUtc { get; init; }
    public DateTime ImportedDateUtc { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public long SizeKb { get; init; }
    

    public Dictionary<string,PhotoSummary>? PhotoSummaries { get; set; } 

    public List<ExifData>? Metadata { get; init; }

    public FeatureCollection? LocationInformation { get; set; }

    public Thumbnail? Thumbnails { get; init; }
}

public class ExifData
{
    [MaxLength(250)]
    public required string Name { get; set; }
    [MaxLength(500)]
    public string? Value { get; set; }
}