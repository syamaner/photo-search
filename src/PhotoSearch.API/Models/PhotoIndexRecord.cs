namespace PhotoSearch.API.Models;

public class PhotoIndexRecord
{    
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public required string FilePath { get; set; }
    public required DateTimeOffset CaptureDate { get; set; }
    public ReadOnlyMemory<float>? Vector { get; set; }
}