using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using PhotoSearch.Ollama;

namespace PhotoSearch.AppHost.DashboardCommands;

internal static class OllamaCommandExtensions
{
    public static IResourceBuilder<OllamaResource> WithOllamaDownloadCommand(
        this IResourceBuilder<OllamaResource> builder)
    {
        builder.WithCommand(
            name: "download-models",
            displayName: "Download Models",
            executeCommand: context => OnOllamaDownloadCommandAsync(builder, context),
            updateState: OnUpdateResourceState,
            iconName: "ArrowDownload",
            iconVariant: IconVariant.Filled);

        return builder;
    }
    private static async Task<bool> HasModelAsync(OllamaApiClient ollamaClient, string model, CancellationToken cancellationToken)
    {
        var localModels = await ollamaClient.ListLocalModelsAsync(cancellationToken);
        return localModels.Any(m => m.Name.StartsWith(model));
    }
    private static async Task<ExecuteCommandResult> OnOllamaDownloadCommandAsync(
        IResourceBuilder<OllamaResource> builder,
        ExecuteCommandContext context)
    {
        
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        var notificationService = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();
        
        var url = builder.Resource.GetEndpoints().First().Url;
        var ollamaClient = new OllamaApiClient(new Uri(url!));
        var modelNames = Environment.GetEnvironmentVariable("OLLAMA_MODELS_TO_DOWNLOAD")
            ?.Split([','], StringSplitOptions.RemoveEmptyEntries);
        
        if (modelNames == null || modelNames.Length == 0)
        {
            return new ExecuteCommandResult()
            {
                Success = false,
                ErrorMessage =
                    "OLLAMA_MODELS_TO_DOWNLOAD environment variable is not set. It needs to contain a comma seperated list of model names."
            };
        }

        var models = await ollamaClient.ListLocalModelsAsync();
        foreach (var model in models)
        {
            logger.LogInformation("Model {MODEL} exists", model.Name);
        }
        foreach (var modelName in modelNames)
        {
            var modelExists = await HasModelAsync(ollamaClient, modelName, context.CancellationToken);
            if (modelExists)
            {
                Console.WriteLine($"Model {modelName} already exists");
                await notificationService.PublishUpdateAsync(builder.Resource,
                    state => state with
                    {
                        State = new ResourceStateSnapshot($"Model {modelName} exists. Skipping download.", KnownResourceStateStyles.Info)
                    });
                continue;
            }
            long percentage = 0;
            var response = ollamaClient.PullModelAsync(new PullModelRequest() { Model = modelName, Stream = true});
            await foreach (var status in response)
            {
                if (status?.Total == 0) continue;
            
                var newPercentage = (long)(status?.Completed / (double)status!.Total * 100)!;
                if (newPercentage == percentage) continue;
                percentage = newPercentage;

                var percentageState = percentage == 0 ? $"Downloading model {modelName}" : $"Downloading model {modelName} {percentage} percent";
                Console.WriteLine(percentageState);
                await notificationService.PublishUpdateAsync(builder.Resource,
                    state => state with
                    {
                        State = new ResourceStateSnapshot(percentageState, KnownResourceStateStyles.Info)
                    });
            }
            
        }

        return CommandResults.Success();
    }

    private static ResourceCommandState OnUpdateResourceState(
        UpdateCommandStateContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Updating resource state: {ResourceSnapshot}",
                context.ResourceSnapshot);
        }

        return context.ResourceSnapshot.HealthStatus is HealthStatus.Healthy
            ? ResourceCommandState.Enabled
            : ResourceCommandState.Disabled;
    }
}