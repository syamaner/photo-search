using System.Text;
using ImageMagick;
using MetadataExtractor;
using Microsoft.Extensions.Logging;
using PhotoSearch.Data.Models;
using Directory = System.IO.Directory;

namespace PhotoSearch.Common;

public class PhotoImporter(ILogger<PhotoImporter> logger, IReverseGeocoder reverseGeocoder) : IPhotoImporter
{
    private readonly List<string> _fileExtensionsToInclude = ["jpg"];
    private const uint ThumbnailWidth = 640, ThumbnailHeight = 800;
    public async Task<List<Photo>> ImportPhotos(string baseDirectory, List<string> existingIds)
    {
        var photos = new List<Photo>();
        foreach (var imageFile in GetImageFiles(baseDirectory))
        {
            try
            {
                var photo = await GetPhotoInformation(imageFile, baseDirectory);
                if (existingIds.Contains(photo.RelativePath))
                {
                    logger.LogInformation("Skipping {ImageFile} as it already exists in the database.", imageFile);
                    continue;
                }
                photos.Add(photo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error importing photo {ImageFile}", imageFile);
            }
        }

        return photos;
    }

    private async Task<Photo> GetPhotoInformation(string fullPath, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            throw new ArgumentException($"Invalid argument. The image file {fullPath} does not exits.",
                nameof(fullPath));
        var metadata = ReadExifTags(fullPath);
        var gpsLocation = MetadataHelper.GetLocation(metadata);
        using var image = new MagickImage(fullPath);
        var photo = new Photo
        {
            Metadata = metadata.Select(x=>new ExifData{Name=x.Key, Value=x.Value}).ToList(),
            RelativePath = fullPath.Replace(baseDirectory, string.Empty),
            ExactPath = fullPath,
            SizeKb = new FileInfo(fullPath).Length / 1024,
            Width = (int)image.Width,
            Height = (int)image.Height,
            Latitude = gpsLocation?.Latitude,
            Longitude = gpsLocation?.Longitude,
            ImportedDateUtc = DateTime.UtcNow,
            FileType = new FileInfo(fullPath).Extension,
            CaptureDateUtc = MetadataHelper.GetImageCaptureTime(metadata),
            Base64Data = await CreateThumbnailBase64(fullPath)
        };
        if (photo is { Latitude: not null, Longitude: not null })
        {
            photo.LocationInformation = await reverseGeocoder.ReverseGeocode(photo.Latitude.Value, photo.Longitude.Value, CancellationToken.None);
        }
        return photo;
    }
    
    private Dictionary<string, string> ReadExifTags(string imagePath)
    {
        var directories = ImageMetadataReader.ReadMetadata(imagePath).ToList();
        try
        {
            var metadataTags = (from tag in directories.SelectMany(x => x.Tags)
                    select new { Name=$"{tag.DirectoryName}-{tag.Name}", tag.Description  })
                .ToDictionary(x => x.Name, x => x.Description);
            return metadataTags;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error reading metadata from {ImagePath}", imagePath);
            throw;
        }
    }
    
  private async Task<string> CreateThumbnailBase64(string fullPath)
    {
        using var image = new MagickImage(fullPath);
        using var thumbnail = image.Clone();
        thumbnail.Resize(new MagickGeometry(ThumbnailWidth, ThumbnailHeight)
        {
            IgnoreAspectRatio = false,
            Greater = false // Only shrink if larger than dimensions
        });
        thumbnail.Format = MagickFormat.Jpg;
        using var memStream = new MemoryStream();
        await thumbnail.WriteAsync(memStream);
        return Convert.ToBase64String(memStream.ToArray());
    }

    private IEnumerable<string> GetImageFiles(string baseDirectory)
    {
        return Directory.EnumerateFiles(baseDirectory, "*", SearchOption.AllDirectories).Where(fileName =>
            _fileExtensionsToInclude.Any(ext => fileName.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));
    }
}
 