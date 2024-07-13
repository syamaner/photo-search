namespace PhotoSearch.Data.Models;

public class Photo
{
    public Photo()
    {
        
    }
    public required string RelativePath { get; set; }
    public required string ExactPath { get; set; }
    public string? PublicUrl { get; set; }
    public required string FileType { get; set; }
    public DateTime? CaptureDateUTC { get; set; }
    public DateTime ImportedDateUTC { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long SizeKb { get; set; }
    public List<PhotoSummary>? PhotoSummaries { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public Thumbnail? Thumbnails { get; set; }
}