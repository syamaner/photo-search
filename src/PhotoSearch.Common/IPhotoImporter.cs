using PhotoSearch.Data.Models;

namespace PhotoSearch.Common;

public interface IPhotoImporter
{
    List<Photo> ImportPhotos(string directory);
}