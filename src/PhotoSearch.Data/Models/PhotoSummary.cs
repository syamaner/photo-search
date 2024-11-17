namespace PhotoSearch.Data.Models;

public class PhotoSummary
{
    public DateTimeOffset DateGenerated { get; set; }
    public required string Description { get; set; }

    public List<string>? ObjectClasses { get; set; }
    public List<string>? Categories { get; set; }

    public PromptSummary? PromptSummary { get; set; }
}

public record PromptSummary(List<string> Prompts, string Model,TimeSpan TotalDuration, Dictionary<string,object> Parameters);
