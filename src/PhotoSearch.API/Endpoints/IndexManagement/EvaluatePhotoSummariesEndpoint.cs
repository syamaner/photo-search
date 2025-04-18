using FastEndpoints;
using MassTransit;
using PhotoSearch.Common.Contracts;

namespace PhotoSearch.API.Endpoints.IndexManagement;

public class EvaluatePhotoSummariesEndpoint(IBus bus) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/photos/evaluate");
        AllowAnonymous();
        Description(builder => builder.WithName("EvaluatePhotoSummaries"));
        //  .WithOpenApi());
    }

    public override async Task HandleAsync(CancellationToken c)
    { 
        await bus.Publish(new EvaluateModelSummaries(), c);
        await SendAsync("Message sent!", cancellation: c);
    }
}