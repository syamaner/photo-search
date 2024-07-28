using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using OllamaSharp;

namespace PhotoSearch.Ollama;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Reference: https://raygun.com/blog/enhancing-aspire-with-ai-with-ollama/
/// </summary>
public class OllamaResourceLifecycleHook(ResourceNotificationService notificationService)
    : IDistributedApplicationLifecycleHook
{
    public async Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        foreach (var resource in appModel.Resources.OfType<OllamaResource>())
        {
            Console.WriteLine($"Verifying if models are downloaded for model for {resource.Name}");
            await notificationService.PublishUpdateAsync(resource, resource.Name,
                state => state with
                {
                    State = new ResourceStateSnapshot("Initialising", KnownResourceStateStyles.Info)
                });

            DownloadModel(resource, cancellationToken);
        }
    }
    
    private void DownloadModel(OllamaResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.ModelName))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(cancellationToken);
                
                await notificationService.PublishUpdateAsync(resource, 
                    state => state with
                    {
                        State = new ResourceStateSnapshot($"Connection string: {connectionString}", KnownResourceStateStyles.Info)
                    });

                var ollamaClient = new OllamaApiClient(new Uri(connectionString!));
                while (!await IsOllamaReady(ollamaClient, 10, cancellationToken))
                {
                    await notificationService.PublishUpdateAsync(resource,
                        state => state with
                        {
                            State = new ResourceStateSnapshot("Waiting for ollama to start", KnownResourceStateStyles.Info)
                        });
                }
                
                var model = resource.ModelName;

                var hasModel = await HasModelAsync(ollamaClient, model, cancellationToken);
              
                await notificationService.PublishUpdateAsync(resource,
                    state => state with
                    {
                        State = new ResourceStateSnapshot($"hasModel: {hasModel}", KnownResourceStateStyles.Info)
                    });
                if (!hasModel)
                {
                    await notificationService.PublishUpdateAsync(resource,
                        state => state with
                        {
                            State = new ResourceStateSnapshot($"Starting model download : {resource.ModelName}", KnownResourceStateStyles.Info)
                        });
                    await PullModel(resource, ollamaClient, model, cancellationToken);
                }

                await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot("Running", KnownResourceStateStyles.Success) });
            }
            catch (Exception ex)
            {
               await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot(ex.Message, KnownResourceStateStyles.Error) });
            }

        }, cancellationToken);
    }

    private static async Task<bool> IsOllamaReady(OllamaApiClient ollamaClient, int maxAttempts = 5, CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var isRunning=false;
        while (attempt <= maxAttempts && !isRunning )
        {
            try
            {
                attempt++;
                isRunning = await ollamaClient.IsRunning(cancellationToken);
            }
            catch (Exception)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        return isRunning;
    }
    private async Task<bool> HasModelAsync(OllamaApiClient ollamaClient, string model, CancellationToken cancellationToken)
    {
        var localModels = await ollamaClient.ListLocalModels(cancellationToken);
        return localModels.Any(m => m.Name.StartsWith(model));
    }
    private async Task PullModel(OllamaResource resource, OllamaApiClient ollamaClient, string model, CancellationToken cancellationToken)
    {
        await notificationService.PublishUpdateAsync(resource, state => state with { State = new ResourceStateSnapshot("Downloading model", KnownResourceStateStyles.Info) });

        long percentage = 0;

        await ollamaClient.PullModel(model, async status =>
        {
            if (status.Total != 0)
            {
                var newPercentage = (long)(status.Completed / (double)status.Total * 100);
                if (newPercentage != percentage)
                {
                    percentage = newPercentage;

                    var percentageState = percentage == 0 ? "Downloading model" : $"Downloading model {percentage} percent";
                    Console.WriteLine(percentageState);
                    await notificationService.PublishUpdateAsync(resource,
                        state => state with
                        {
                            State = new ResourceStateSnapshot(percentageState, KnownResourceStateStyles.Info)
                        });
                }
            }
        }, cancellationToken);
    }
}