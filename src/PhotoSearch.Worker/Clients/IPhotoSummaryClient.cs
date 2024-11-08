using PhotoSearch.Data.Models;

namespace PhotoSearch.Worker.Clients;

public interface IPhotoSummaryClient
{
    Task<PhotoSummary> SummarisePhoto(string modelName, string imagePath);
    bool CanHandle(string modelName);
}