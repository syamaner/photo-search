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
    private static readonly string[] SupportedModels =
    [
        "o1-preview", "o1-preview-2024-09-12", "gpt-4o-mini",
        "gpt-4o", "gpt-4o-2024-11-20", "gpt-4o-mini-2024-07-18"
    ];
    public override async Task HandleAsync(CancellationToken c)
    {
        var response = await ollamaApiClient.ListLocalModelsAsync(c);
        var modelNames = response.Select(x => x.Name).ToList();
        modelNames.Add("Florence-2-large");

        modelNames.AddRange(SupportedModels);
        
        await SendAsync(modelNames, cancellation: c);    
    }
}