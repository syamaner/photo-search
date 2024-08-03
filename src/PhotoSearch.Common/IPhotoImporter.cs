using PhotoSearch.Data.Models;

namespace PhotoSearch.Common;

public interface IPhotoImporter
{
    Task<List<Photo>>ImportPhotos(string directory, List<string> existingIds);
}