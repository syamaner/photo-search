using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace PhotoSearch.Ollama;

public static class OllamaResourceBuilderExtensions
{
    private const int OllamaContainerPort = 11434;

    public static IResourceBuilder<OllamaResource> AddOllama(this IDistributedApplicationBuilder builder,
        string modelName = "llava:34b",
        string ollamaTag = "latest",
        string hostIpAddress = "192.168.0.158",
        string name = "Ollama", int? hostPort = 11438)
    {
        var ollamaResource = new OllamaResource(name, modelName, hostIpAddress, hostPort.ToString()!);
        builder.Services.TryAddLifecycleHook<OllamaResourceLifecycleHook>();
        
        return builder.AddResource(ollamaResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "ollama/ollama", Tag = ollamaTag })
            .PublishAsContainer()
            .WithVolume("ollama", "/root/.ollama")
            .WithExternalHttpEndpoints()
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:{hostPort}:{OllamaContainerPort}")
            .WithContainerRuntimeArgs("--gpus=all");
    }
}