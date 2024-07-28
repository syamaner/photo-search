using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OllamaSharp;
using OllamaSharp.Models;

namespace PhotoSearch.Ollama;

public class OllamaHealthCheck(string url, string modelName) : IHealthCheck
{
    private readonly OllamaApiClient _ollamaClient = new(new Uri(url!));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        List<Model>? localModels = null;
        try
        {
            var ollamaRunning = await _ollamaClient.IsRunning(cancellationToken);
            if(!ollamaRunning) 
                return HealthCheckResult.Unhealthy("Ollama is not running yet.");
            localModels = (await _ollamaClient.ListLocalModels(cancellationToken))?.ToList();
        }
        catch
        {
            // ignored
        }
        var modelExists =localModels?.Any(m => m.Name.StartsWith(modelName));
        var healthy = modelExists.HasValue  && modelExists.Value;

        return healthy ? HealthCheckResult.Healthy("The check succeeded.") : HealthCheckResult.Unhealthy("Ollama health check failed.");
    }
}