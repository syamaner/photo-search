using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace PhotoSearch.Ollama;

public static class OllamaResourceBuilderExtensions
{
    private const int OllamaContainerPort = 11434;

    public static IResourceBuilder<OllamaResource> AddOllama(this IDistributedApplicationBuilder builder,
        string modelName = "llava:7b",
        string ollamaTag = "latest",
        string hostIpAddress= "",
        string name = "Ollama", int? hostPort = 11438)
    {
        var ollamaResource = new OllamaResource(name, modelName, hostIpAddress, hostPort.ToString()!);
        builder.Services.TryAddLifecycleHook<OllamaResourceLifecycleHook>();
        
        var ollamaResourceBuilder = builder.AddResource(ollamaResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "ollama/ollama", Tag = ollamaTag })
            .PublishAsContainer()
            .WithVolume("ollama", "/root/.ollama")
            .WithExternalHttpEndpoints()
            .WithContainerRuntimeArgs("--gpus=all");

        
        if (!string.IsNullOrWhiteSpace(hostIpAddress))
        {
            ollamaResourceBuilder
                .WithContainerRuntimeArgs("-p", $"0.0.0.0:{hostPort}:{OllamaContainerPort}");
        }
        else
        {
            ollamaResourceBuilder.WithHttpEndpoint(hostPort, OllamaContainerPort);
        }

        return ollamaResourceBuilder;
    }
}