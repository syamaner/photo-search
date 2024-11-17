using System.Net.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PhotoSearch.Common;

namespace PhotoSearch.Nominatim;

public class NominatimHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;

    public NominatimHealthCheck(string url)
    {
        _httpClient = HttpClientFactory.Create();
        _httpClient.BaseAddress = new Uri(url);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    { 
        var ready = await IsServerReady(cancellationToken);
        Console.WriteLine(ready ? "Nominatim container is ready." : "Nominatim container is not ready yet.");
        return ready
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    }

    private async Task<bool> IsServerReady(CancellationToken cancellationToken = default)
    {
        const string searchUrl = "/search.php?q=avenue%20pasteur";
        try
        {
            var status =
                await _httpClient.GetFromJsonAsync<NominatimStatusResponse>("/status?format=json", cancellationToken);
            if (status is not { Status: 0 })
            {
                return false;
            }
            var searchResponse = await _httpClient.GetAsync(searchUrl, cancellationToken);
            return searchResponse.IsSuccessStatusCode;
        }
        catch
        {
            // ignored
        }

        return false;
    }
}
