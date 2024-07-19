using MetadataExtractor;
using PhotoSearch.Common;

namespace PhotoSearch.Tests;

public class MetadataHelperTests
{
    [Theory()]
    [InlineData("TestData/1.jpg", "2005-07-15T19:53:19Z")]
    [InlineData("TestData/2.jpg", "2005-11-10T22:47:36Z")]
    [InlineData("TestData/3.jpg", "2007-04-22T19:38:03Z")]
    [InlineData("TestData/4.jpg", "2007-05-03T12:47:50Z")]
    [InlineData("TestData/5.jpg", "2009-10-01T17:33:24Z")]
    [InlineData("TestData/6.jpg", "2010-07-10T13:22:23Z")]
    [InlineData("TestData/7.jpg", "2010-12-11T21:07:05Z")]
    [InlineData("TestData/8.jpg", "2016-01-29T21:16:00Z")]
    [InlineData("TestData/9.jpg", "2016-10-28T21:01:31Z")]
    [InlineData("TestData/10.jpg", "2017-07-07T19:18:42Z")]
    [InlineData("TestData/11.jpg", "2019-05-13T10:26:45Z")]
    [InlineData("TestData/12.jpg", "2022-06-13T21:45:12Z")]
    [InlineData("TestData/13.jpg", "2024-05-11T12:05:51Z")]
    [InlineData("TestData/14.jpg", "2024-02-24T12:25:19Z")]
    public void ParsesTheCorrectCaptureDateAndTimeInUtc(string imagePath, string expectedDateTimeUtcString)
    {
        var expectedDateTimeUtc = DateTime.Parse(expectedDateTimeUtcString).ToUniversalTime();
        var metadata = ReadExifTags(imagePath);
        var actualDateTime = MetadataHelper.GetImageCaptureTime(metadata);
        Assert.Equal(expectedDateTimeUtc, actualDateTime); 
    }
    
    [Theory()]
    [InlineData("TestData/1.jpg", null,null)]
    [InlineData("TestData/2.jpg", null,null)]
    [InlineData("TestData/3.jpg", null,null)]
    [InlineData("TestData/4.jpg", 23.676725000000001,120.79585833333333)]
    [InlineData("TestData/5.jpg", 51.491127777777777,-0.22474722222222224)]
    [InlineData("TestData/6.jpg", 52.948074999999996,0.90858611111111109)]
    [InlineData("TestData/7.jpg", 23.980802777777775,120.68119166666668)]
    [InlineData("TestData/8.jpg", null,null)]
    [InlineData("TestData/9.jpg", null,null)]
    [InlineData("TestData/10.jpg", 48.924830555555552,2.3607972222222222)]
    [InlineData("TestData/11.jpg", 46.80278055555555,9.8152249999999999)]
    [InlineData("TestData/12.jpg", 54.58999166666667,4.0835583333333334)]
    [InlineData("TestData/13.jpg", 52.659380555555558,-0.69077222222222223)]
    [InlineData("TestData/14.jpg", 24.186219444444443,120.65206944444445)]
    public void ParseCorrectGeoLocation(string imagePath, double? latitude, double? longitude)
    {
        var metadata = ReadExifTags(imagePath);
        var gpsLocation = MetadataHelper.GetLocation(metadata);
        Assert.Equal(latitude, gpsLocation?.Latitude);
        Assert.Equal(longitude, gpsLocation?.Longitude);
    }
    
    private Dictionary<string, string> ReadExifTags(string imagePath)
    {
        var directories = ImageMetadataReader.ReadMetadata(imagePath).ToList();

        var metadataTags = (from tag in directories.SelectMany(x => x.Tags)
                select new { Name = $"{tag.DirectoryName}-{tag.Name}", tag.Description })
            .ToDictionary(x => x.Name, x => x.Description);
        return metadataTags;

    }

}