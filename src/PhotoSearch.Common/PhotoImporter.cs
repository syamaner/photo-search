using System.Text;
using ImageMagick;
using MetadataExtractor;
using Microsoft.Extensions.Logging;
using PhotoSearch.Data.Models;
using Directory = System.IO.Directory;

namespace PhotoSearch.Common;

public class PhotoImporter(ILogger<PhotoImporter> logger) : IPhotoImporter
{
    private readonly List<string> _fileExtensionsToInclude = ["jpg"];
    private readonly ILogger<PhotoImporter> _logger = logger;

    public List<Photo> ImportPhotos(string baseDirectory)
    {
        return GetImageFiles(baseDirectory).Select(file => GetPhotoInformation(file, baseDirectory)).ToList();
    }

    private Photo GetPhotoInformation(string fullPath, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(fullPath) || !File.Exists(fullPath))
            throw new ArgumentException($"Invalid argument. The image file {fullPath} does not exits.",
                nameof(fullPath));
        var metadata = ReadExifTags(fullPath);
        var gpsLocation = MetadataHelper.GetLocation(metadata);
        using var image = new MagickImage(fullPath);
        var photo = new Photo
        {
            Metadata = metadata,
            RelativePath = fullPath.Replace(baseDirectory, string.Empty),
            ExactPath = fullPath,
            SizeKb = new FileInfo(fullPath).Length / 1024,
            Width = image.Width,
            Height = image.Height,
            Latitude = gpsLocation?.Latitude,
            Longitude = gpsLocation?.Longitude,
            ImportedDateUTC = DateTime.UtcNow,
            FileType = new FileInfo(fullPath).Extension,
            CaptureDateUTC = MetadataHelper.GetImageCaptureTime(metadata)
        };
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
            _logger.LogError(ex, "Error reading metadata from {ImagePath}", imagePath);
            throw;
        }
    }
    
    private IEnumerable<string> GetImageFiles(string baseDirectory)
    {
        return Directory.EnumerateFiles(baseDirectory, "*", SearchOption.AllDirectories).Where(fileName =>
            _fileExtensionsToInclude.Any(ext => fileName.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase)));
    }
}