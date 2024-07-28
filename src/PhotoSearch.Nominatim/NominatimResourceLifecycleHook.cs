using System.Net.Http.Json;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using PhotoSearch.Common;

namespace PhotoSearch.Nominatim;

public class NominatimResourceLifecycleHook(ResourceNotificationService notificationService)
    : IDistributedApplicationLifecycleHook
{
    public async Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        foreach (var resource in appModel.Resources.OfType<NominatimResource>())
        {
            Console.WriteLine($"Verifying if the maps are downloaded for {resource.Name}");
            await notificationService.PublishUpdateAsync(resource,
                state => state with
                {
                    State = new ResourceStateSnapshot("Initialising", KnownResourceStateStyles.Info)
                });

            DownloadMaps(resource, cancellationToken);
        }
 
    }

    private void DownloadMaps(NominatimResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.MapsDownloadUrl))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
            using var httpClient = HttpClientFactory.Create();
            httpClient.BaseAddress = new Uri(connectionString!);
            
            await notificationService.PublishUpdateAsync(resource,
                state => state with
                {
                    State = new ResourceStateSnapshot($"Connection string: {connectionString}", KnownResourceStateStyles.Info)
                });
            
            var isReady = false;
            while (!isReady)
            {
                isReady = await IsServerReady(httpClient, cancellationToken);
                await notificationService.PublishUpdateAsync(resource,
                    state => state with
                    {
                        State = new ResourceStateSnapshot("Waiting for Nominatim to start", KnownResourceStateStyles.Info)
                    });
                if (!isReady)
                {
                    await Task.Delay(2500, cancellationToken);
                }
            }
            
            await notificationService.PublishUpdateAsync(resource,
                state => state with
                {
                    State = new ResourceStateSnapshot("Nominatim is ready", KnownResourceStateStyles.Success)
                });
        }, cancellationToken);
    }

    private static async Task<bool> IsServerReady(HttpClient nominatimWebInterface, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status =
                await nominatimWebInterface.GetFromJsonAsync<NominatimStatusResponse>("/status?format=json", cancellationToken);
            return status is { Status: 0 };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
}