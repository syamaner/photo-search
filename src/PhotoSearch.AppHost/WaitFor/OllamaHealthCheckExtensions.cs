using System;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.Ollama;

namespace PhotoSearch.AppHost.WaitFor;

/// <summary>
///
/// Reference: https://github.dev/davidfowl/WaitForDependenciesAspire
/// </summary>
public static class OllamaHealthCheckExtensions
{
    public static IResourceBuilder<OllamaResource> WithHealthCheck(
        this IResourceBuilder<OllamaResource> builder)
    {
        return builder.WithAnnotation(HealthCheckAnnotation.Create(cs =>
        {
            Console.WriteLine(cs);
            return new OllamaHealthCheck(cs, builder.Resource.ModelName);
        }));
    }
}