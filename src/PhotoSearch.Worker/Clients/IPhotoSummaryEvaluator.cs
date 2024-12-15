using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public interface IPhotoSummaryEvaluator
{
    Task<PhotoSummaryScore> EvaluatePhotoSummary(string base64Image, ImageSummaryEvaluationRequest summary); 
}