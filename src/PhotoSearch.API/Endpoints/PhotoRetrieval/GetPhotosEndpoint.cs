using MongoDB.Driver;
using PhotoSearch.Data.Models;

namespace PhotoSearch.API.Endpoints.PhotoRetrieval;
using FastEndpoints;

public class GetPhotosEndpoint(IMongoCollection<Photo> collection): EndpointWithoutRequest<List<Models.GetPhotosResponse>>
{
    public override void Configure()
    {
        Get("/photos/{modelName}");
        AllowAnonymous();
        Description(builder=>builder.WithName("GetPhotos")
            .WithOpenApi());
        
    }

    public override async Task HandleAsync(CancellationToken c)
    { 
        var photos = await collection.AsQueryable().ToListAsync(cancellationToken: c);

        var results = photos.Select(x => new Models.GetPhotosResponse
        (
            x.Id,
            x.PhotoSummaries?.ToDictionary(x => x.Key, x => x.Value.Description)
            ?? new Dictionary<string, string>(),
            x.Latitude,
            x.Longitude,
            x.LocationInformation?.Features?.FirstOrDefault()?.Properties?.DisplayName
        )).ToList();

        await SendAsync(results,200, cancellation: c);
    }
    
}