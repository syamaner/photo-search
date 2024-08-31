using System.Net.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PhotoSearch.Common;

namespace PhotoSearch.MapTileServer;

public class MapTileServerHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    public MapTileServerHealthCheck(string url)
    {
        _httpClient = HttpClientFactory.Create();
        _httpClient.BaseAddress = new Uri(url);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    { 
        var ready = await IsServerReady(cancellationToken);
        Console.WriteLine(ready ? "Map Tile Server container is ready." : "Map Tile Server container is not ready yet.");
        return ready
            ? HealthCheckResult.Healthy("Map Tile Server is ready.")
            : HealthCheckResult.Unhealthy("Map Tile Server is not ready yet.");
    }

    private async Task<bool> IsServerReady(CancellationToken cancellationToken = default)
    {
        try
        {
            var status =
                await _httpClient.GetAsync("/", cancellationToken);
            
            return status is { IsSuccessStatusCode: true };
        }
        catch
        {
            // ignored
        }

        return false;
    }
}