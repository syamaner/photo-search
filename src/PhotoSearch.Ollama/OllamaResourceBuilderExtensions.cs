using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace PhotoSearch.Ollama;

/// <summary>
/// Reference: https://raygun.com/blog/enhancing-aspire-with-ai-with-ollama/
/// </summary>
public static class OllamaResourceBuilderExtensions
{
 
    public static IResourceBuilder<OllamaResource> AddOllama(this IDistributedApplicationBuilder builder,
        string modelName,
        string hostIpAddress= "",
        bool useGpu = true,
        string ollamaTag = "latest",
        string name = "Ollama", 
        int? hostPort = 11438, 
        int ollamaContainerPort = 11434)
    {
        var ollamaResource = new OllamaResource(name, modelName, hostIpAddress, hostPort.ToString()!);

        var ollamaResourceBuilder = builder.AddResource(ollamaResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "ollama/ollama", Tag = ollamaTag })
            .PublishAsContainer()
            .WithHttpEndpoint(hostPort, ollamaContainerPort, isProxied:false)
            .WithVolume("ollamas", "/root/.ollama")
            .WithExternalHttpEndpoints();
        
        if (useGpu)
        {
            ollamaResourceBuilder.WithContainerRuntimeArgs("--gpus=all");
        }
        
        builder.Services.TryAddLifecycleHook<OllamaResourceLifecycleHook>();

        return ollamaResourceBuilder;
    }
}