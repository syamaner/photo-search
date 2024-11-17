using FastEndpoints;
using OllamaSharp;

namespace PhotoSearch.API.Endpoints.IndexManagement;

public class ListModelsEndpoint(IOllamaApiClient ollamaApiClient) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/models/list");
        AllowAnonymous();
        Description(builder=>builder.WithName("ListModels")
            .WithOpenApi());
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var response = await ollamaApiClient.ListLocalModelsAsync(c);
        List<string> modelNames = response.Select(x => x.Name).ToList();
        
        await SendAsync(modelNames, cancellation: c);    
    }
}