using Microsoft.AspNetCore.Cors;
using MongoDB.Driver;
using PhotoSearch.Data.Models;
using YamlDotNet.Core.Tokens;

namespace PhotoSearch.API.Endpoints.PhotoRetrieval;
using FastEndpoints;

public class GetPhotosEndpoint(IMongoCollection<Photo> collection): EndpointWithoutRequest<List<Models.GetPhotosResponse>>
{
    public override void Configure()
    {
        Get("/api/photos");
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
            x.PhotoSummaries?.ToDictionary(x =>
                    x.Key,
                x =>
                    new Models.ModelResponse(x.Value?.Description, x.Value?.Categories ?? [], x.Value?.ObjectClasses ?? []))
            ?? new Dictionary<string, Models.ModelResponse>()
            ,
            x.Latitude,
            x.Longitude,
            x.LocationInformation?.Features?.FirstOrDefault()?.Properties?.DisplayName,
            x.Base64Data
        )).ToList();

        await SendAsync(results,200, cancellation: c);
    }
    
}