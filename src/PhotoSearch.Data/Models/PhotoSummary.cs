namespace PhotoSearch.Data.Models;

public class PhotoSummary
{
    public required string Model { get; set; }
    public DateTimeOffset DateGenerated { get; set; }
    public required string Description { get; set; }
    
    public List<string>? ObjectClasses { get; set; }
    public List<string>? Categoties { get; set; }
}