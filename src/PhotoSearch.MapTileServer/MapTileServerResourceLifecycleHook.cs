using System.Net.Http.Json;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;
using PhotoSearch.Common;

namespace PhotoSearch.MapTileServer;

public class MapTileServerResourceLifecycleHook(
    ResourceNotificationService notificationService,
    ILogger<MapTileServerResourceLifecycleHook> logger)
    : IDistributedApplicationLifecycleHook
{
    public async Task BeforeStartAsync(DistributedApplicationModel appModel,
        CancellationToken cancellationToken = default)
    {
        foreach (var resource in appModel.Resources.OfType<MapTileServerResource>())
        {
            Console.WriteLine($"Verifying if the maps are downloaded for {resource.Name}");
            await notificationService.PublishUpdateAsync(resource, resource.Name,
                state => state with
                {
                    State = new ResourceStateSnapshot("Initialising", KnownResourceStateStyles.Info)
                });
            WatchNominatimStartup(resource, cancellationToken);
        }
    }

    private void WatchNominatimStartup(MapTileServerResource resource, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
            using var httpClient = HttpClientFactory.Create();
            httpClient.BaseAddress = new Uri(connectionString!);

            await notificationService.PublishUpdateAsync(resource, resource.Name,
                state => state with
                {
                    State = new ResourceStateSnapshot($"Connection string: {connectionString}",
                        KnownResourceStateStyles.Info)
                });

            var isReady = false;
            while (!isReady)
            {
                isReady = await IsServerReady(httpClient, cancellationToken);
                await notificationService.PublishUpdateAsync(resource, resource.Name,
                    state => state with
                    {
                        State = new ResourceStateSnapshot("Waiting for Map Tile Server to start",
                            KnownResourceStateStyles.Info)
                    });
                if (!isReady)
                {
                    await Task.Delay(2500, cancellationToken);
                }
            }

            await notificationService.PublishUpdateAsync(resource, resource.Name,
                state => state with
                {
                    State = new ResourceStateSnapshot("Map Tile Server is ready", KnownResourceStateStyles.Success)
                });
        }, cancellationToken);
    }

    private async Task<bool> IsServerReady(HttpClient nominatimWebInterface,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var status =
                await nominatimWebInterface.GetAsync("/",
                    cancellationToken);
            return status.IsSuccessStatusCode;
        }
        catch (Exception _)
        {
            //logger.LogError(e, "Failed to check Nominatim status");
            // ignored
        }

        return false;
    }
}