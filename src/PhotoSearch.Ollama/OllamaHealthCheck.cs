using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OllamaSharp;
using OllamaSharp.Models;

namespace PhotoSearch.Ollama;

public class OllamaHealthCheck : IHealthCheck
{
    public OllamaHealthCheck(string url, string modelName)
    {
        _url = url;
        _modelName = modelName;
        _ollamaClient = new(new Uri(_url));
        Console.WriteLine($"Url: {_url}  model name: {_modelName}");
    }

    private readonly OllamaApiClient _ollamaClient;
    private readonly string _url;
    private readonly string _modelName;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Url: {_url}  model name: {_modelName}");
        List<Model>? localModels = null;
        try
        {
            var ollamaRunning = await _ollamaClient.IsRunningAsync(cancellationToken);
            if(!ollamaRunning) 
                return HealthCheckResult.Unhealthy("Ollama is not running yet.");
            localModels = (await _ollamaClient.ListLocalModelsAsync(cancellationToken))?.ToList();
        }
        catch
        {
            // ignored
        }
        var modelExists =localModels?.Any(m => m.Name.StartsWith(_modelName));
        var healthy = modelExists.HasValue  && modelExists.Value;

        return healthy ? HealthCheckResult.Healthy("The check succeeded.") : HealthCheckResult.Unhealthy("Ollama health check failed.");
    }
}