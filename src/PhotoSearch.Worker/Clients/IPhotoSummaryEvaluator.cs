using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public interface IPhotoSummaryEvaluator
{
    //public record PhotoSummaryScore(double Score, string Justification, string Method);
    Task<PhotoSummaryScore> EvaluatePhotoSummary(string base64Image, ImageSummaryEvaluationRequest summary); 
}
public record ImageSummaryEvaluationRequest(string Summary, List<string> ObjectClasses, List<string> Categories);