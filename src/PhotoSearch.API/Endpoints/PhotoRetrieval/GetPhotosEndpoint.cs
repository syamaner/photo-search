using Microsoft.EntityFrameworkCore;

namespace PhotoSearch.API.Endpoints.PhotoRetrieval;
using FastEndpoints;
using Data;


public class GetPhotosEndpoint(PhotoSearchContext photoSearchContext): Endpoint<Models.GetPhotosRequest>
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
        var photos = await photoSearchContext.Photos.ToListAsync(cancellationToken: c);
        var results = photos.OrderBy(x => new Random(Environment.TickCount).Next())
            .Take(5).Select(x => new
            {
               x.RelativePath,
               Descriptions=x.PhotoSummaries
            }).ToList();

    
        await SendAsync(results,200, cancellation: c);
    }
    
}