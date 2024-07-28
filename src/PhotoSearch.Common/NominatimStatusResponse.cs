namespace PhotoSearch.Common;

public class NominatimStatusResponse
{
    public int Status { get; set; }
    public string Message { get; set; } = null!;
    public DateTime DataUpdated { get; set; }
    public string SoftwareVersion { get; set; } = null!;
    public string DatabaseVersion { get; set; } = null!;
}
 