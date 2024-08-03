using FastEndpoints;
using MassTransit;
using PhotoSearch.Common.Contracts;

namespace PhotoSearch.API.Endpoints.IndexManagement;

public class IndexPhotosEndpoint(IBus bus) : Endpoint<IndexPhotosRequest>
{
    public override void Configure()
    {
        Get("/photos/index/{directory}");
        AllowAnonymous();
        Description(builder=>builder.WithName("IndexPhotos")
            .WithOpenApi());
        
    }

    public override async Task HandleAsync(IndexPhotosRequest r, CancellationToken c)
    {
        await bus.Publish(new ImportPhotos(Uri.UnescapeDataString(r.Directory)), c);
        await SendAsync("Index command sent", cancellation: c);
    }
}