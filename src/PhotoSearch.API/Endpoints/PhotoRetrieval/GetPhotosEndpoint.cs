using MongoDB.Driver;
using PhotoSearch.Data.Models;

namespace PhotoSearch.API.Endpoints.PhotoRetrieval;
using FastEndpoints;

public class GetPhotosEndpoint(IMongoCollection<Photo> collection): Endpoint<Models.GetPhotosRequest>
{
    public override void Configure()
    {
        Get("/photos/{modelName}");
        AllowAnonymous();
        Description(builder=>builder.WithName("GetPhotos")
            .WithOpenApi());
        
    }

    public override async Task HandleAsync(Models.GetPhotosRequest r, CancellationToken c)
    { 
        var photos = await collection.AsQueryable().ToListAsync(cancellationToken: c);
        
        var results = photos.OrderBy(x => new Random(Environment.TickCount).Next())
            .Take(5).Select(x => new
            {
               x.RelativePath,
               Descriptions=x.PhotoSummaries
            }).ToList();

    
        await SendAsync(results,200, cancellation: c);
    }
    
}