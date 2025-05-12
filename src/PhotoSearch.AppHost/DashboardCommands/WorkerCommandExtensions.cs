using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace PhotoSearch.AppHost.DashboardCommands;

internal static class WorkerCommandExtensions
{
    [Obsolete("Obsolete")]
    public static IResourceBuilder<ProjectResource> WithSummariseCommand(
        this IResourceBuilder<ProjectResource> builder)
    {
        builder.WithCommand(
            name: "summarise-images",
            displayName: "Summarise Images",
            executeCommand: context => OnSummariseCommandAsync(builder, context),
            updateState: OnUpdateResourceState,
            iconName: "Image",
            iconVariant: IconVariant.Filled);

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnSummariseCommandAsync(
        IResourceBuilder<ProjectResource> builder,
        ExecuteCommandContext context)
    {
        var url = builder.Resource.GetEndpoints().First().Url;
        using var httpclient = new HttpClient();
        httpclient.BaseAddress = new Uri(url);
        var modelNames = Environment.GetEnvironmentVariable("BATCH_UPDATE_MODELS")
            ?.Split([','], StringSplitOptions.RemoveEmptyEntries);
        if (modelNames == null || modelNames.Length == 0)
        {
            return new ExecuteCommandResult()
            {
                Success = false,
                ErrorMessage =
                    "BATCH_UPDATE_MODELS environment variable is not set. It needs to contain a comma seperated list of model names."
            };
        }

        httpclient.BaseAddress = new Uri(url);

        var queryString = modelNames.Aggregate(string.Empty, (current, name) =>
            QueryHelpers.AddQueryString(current, "modelNames", name));

        var result = await httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Get,
            $"/api/photos/summarise/batch{queryString}"));

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