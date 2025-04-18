using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

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
        string ollamaTag = "0.6.6-rc2",
        string name = "Ollama", 
        int? hostPort = 11438, 
        int ollamaContainerPort = 11434)
    {
        var ollamaResource = new OllamaResource(name, modelName, hostIpAddress, hostPort.ToString()!);
        
        builder.Services.AddHealthChecks().AddTypeActivatedCheck<OllamaHealthCheck>("ollama-healthcheck",ollamaResource.ConnectionStringExpression.ValueExpression,modelName);
        
        var ollamaResourceBuilder = builder.AddResource(ollamaResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "ollama/ollama", Tag = ollamaTag })
            .WithContainerName("Ollama")
            .PublishAsContainer()
            .WithHttpEndpoint(hostPort, ollamaContainerPort, isProxied:false)
            .WithHealthCheck("ollama-healthcheck")
            .WithVolume("ollamas", "/root/.ollama") 
            .WithLifetime(ContainerLifetime.Session)         
            .WithExternalHttpEndpoints();
        
        if (useGpu)
        {
            ollamaResourceBuilder.WithContainerRuntimeArgs("--gpus=all");
        }
        
        builder.Services.TryAddLifecycleHook<OllamaResourceLifecycleHook>();

        return ollamaResourceBuilder;
    }
}