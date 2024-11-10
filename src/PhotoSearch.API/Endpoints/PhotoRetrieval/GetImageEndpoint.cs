using FastEndpoints;
using ImageMagick;
using MongoDB.Driver;
using PhotoSearch.Data.Models;

namespace PhotoSearch.API.Endpoints.PhotoRetrieval;

public class GetImageEndpoint(IMongoCollection<Photo> collection): Endpoint<Models.GetImageRequest>
{
    
    public override void Configure()
    {
        Get("/api/image/{imageId}/{maxWidth}/{maxHeight}");
        AllowAnonymous();
        Description(builder=>builder.WithName("GetImage")
            .WithOpenApi());
        
    }

    public override async Task HandleAsync(Models.GetImageRequest r, CancellationToken c)
    {
        var f = Directory.GetFiles("../PhotoSearch.Worker/test-photos/");
        var photo = collection.AsQueryable().FirstOrDefault(x => x.Id == r.ImageId)!;
        using var image = new MagickImage(Path.Combine("../PhotoSearch.Worker/", photo.ExactPath));

        if(image.Width>r.MaxWidth)
            image.Resize((uint)r.MaxWidth, 0);
        if (image.Height>r.MaxHeight)
            image.Resize(0, (uint)r.MaxHeight);
        
        image.Resize((uint)r.MaxWidth, 0);
        var bytes = image.ToByteArray();
        await using Stream stream = new MemoryStream(bytes);
        await SendStreamAsync(stream, "photo.jpg", bytes.Length, "image/jpeg", cancellation: c);
    }

}