using PhotoSearch.Common.Contracts;
using FastEndpoints;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PhotoSearch.Data;


namespace PhotoSearch.API.Endpoints.IndexManagement;

public class SummarisePhotosEndpoint(IBus bus, PhotoSearchContext photoSearchContext) : Endpoint<SummarisePhotosRequest>
{
    public override void Configure()
    {
        Get("/photos/summarise/{modelName}");
        AllowAnonymous();
        Description(builder=>builder.WithName("SummarisePhotos")
            .WithOpenApi());
    }

    public override async Task HandleAsync(SummarisePhotosRequest r, CancellationToken c)
    {
        var pathsWithoutSummary = await photoSearchContext.Photos
            .Select(p => p.ExactPath).ToListAsync(cancellationToken: c);
        if (pathsWithoutSummary.Count == 0) await SendAsync("no photos to summarise!", cancellation: c);

        await bus.Publish(new SummarisePhotos(pathsWithoutSummary, r.ModelName), c);
        await SendAsync("Message sent!", cancellation: c);
    }
}