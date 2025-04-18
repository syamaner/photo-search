using PhotoSearch.Common.Contracts;
using FastEndpoints;
using MassTransit;
using MongoDB.Driver;
using PhotoSearch.Data.Models;


namespace PhotoSearch.API.Endpoints.IndexManagement;

public class SummarisePhotosEndpoint(IBus bus, IMongoCollection<Photo> collection ) : Endpoint<SummarisePhotosRequest>
{
    public override void Configure()
    {
        Get("/api/photos/summarise/{modelName}");
        AllowAnonymous();
        Description(builder => builder.WithName("SummarisePhotos"));
        //  .WithOpenApi());
    }

    public override async Task HandleAsync(SummarisePhotosRequest r, CancellationToken c)
    {       
            var pathsWithoutSummary = collection.AsQueryable()
                .Select(p => p.ExactPath).ToList();
            if (pathsWithoutSummary.Count == 0) await SendAsync("no photos to summarise!", cancellation: c);

            await bus.Publish(new SummarisePhotos(pathsWithoutSummary, r.ModelName), c);
            await SendAsync("Message sent!", cancellation: c);
    
    }
}