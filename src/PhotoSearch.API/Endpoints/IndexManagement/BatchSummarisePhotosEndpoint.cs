using FastEndpoints;
using MassTransit;
using MongoDB.Driver;
using PhotoSearch.Common.Contracts;
using PhotoSearch.Data.Models;

namespace PhotoSearch.API.Endpoints.IndexManagement;

public class BatchSummarisePhotosEndpoint(IBus bus, IMongoCollection<Photo> collection ) : Endpoint<BatchSummarisePhotosRequest>
{
    public override void Configure()
    {
        Get("/api/photos/summarise/batch");
        AllowAnonymous();
        Description(builder=>builder.WithName("BatchSummarisePhotos"));
//            .WithOpenApi());
    }

    public override async Task HandleAsync(BatchSummarisePhotosRequest r, CancellationToken c)
    { 
        await bus.Publish(new BatchSummarisePhotos(r.ModelNames), c);
        await SendAsync("Message sent!", cancellation: c);
    }
}