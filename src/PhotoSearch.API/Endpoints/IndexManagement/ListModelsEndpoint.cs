using FastEndpoints;
using OllamaSharp;

namespace PhotoSearch.API.Endpoints.IndexManagement;

public class ListModelsEndpoint(IOllamaApiClient ollamaApiClient) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/models/list");
        AllowAnonymous();
        Description(builder => builder.WithName("ListModels"));
        //   .WithOpenApi());
    }
    private static readonly string[] SupportedModels =
    [
        "mistral-small3.1"
    ];
    public override async Task HandleAsync(CancellationToken c)
    {
        var response = await ollamaApiClient.ListLocalModelsAsync(c);
        var modelNames = response.Select(x => x.Name).ToList();

        modelNames.AddRange(SupportedModels);
        
        await SendAsync(modelNames, cancellation: c);    
    }
}