using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace PhotoSearch.AppHost.DashboardCommands;

internal static class JupyterCommandExtensions
{
    private const string NotebookFilename = "comparison.ipynb";

    [Obsolete("Obsolete")]
    public static IResourceBuilder<ContainerResource> WithUploadNoteBookCommand(
        this IResourceBuilder<ContainerResource> builder, string jupyterToken, string jupyterUrl)
    {
        builder.WithCommand(
            name: "upload-notebook",
            displayName: "Upload Notebook",
            executeCommand: context => OnUploadNotebookCommandAsync(builder, context, jupyterToken, jupyterUrl),
            updateState: OnUpdateResourceState,
            iconName: "ArrowUpload",
            iconVariant: IconVariant.Filled);

        return builder;
    }

    private static async Task<ExecuteCommandResult> OnUploadNotebookCommandAsync(
        IResourceBuilder<ContainerResource> builder,
        ExecuteCommandContext context, string jupyterToken, string jupyterUrl)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var destination = Path.Combine(Directory.GetCurrentDirectory(), "Notebooks", NotebookFilename);
        if (!File.Exists(destination))
        {
            return CommandResults.Success();
        }

        var notebookData = await File.ReadAllTextAsync(destination);

        using var httpclient = CreateHttpClient(builder, jupyterToken);

        var jupyterNoteBookPayload = new JupyterNoteBookPayload(Base64Encode(notebookData), "base64", NotebookFilename,
            NotebookFilename, "file");
        var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(jupyterNoteBookPayload),
            Encoding.UTF8, "text/plain");
        var uploadNoteBookHttpRequestMessage =
            new HttpRequestMessage(HttpMethod.Put, $"/api/contents/{NotebookFilename}")
                { Content = requestContent };

        var result = await httpclient.SendAsync(uploadNoteBookHttpRequestMessage);
        if (result.IsSuccessStatusCode)
        {
            logger.LogInformation("Notebook uploaded successfully");
            return CommandResults.Success();
        }

        var errorMessage = await result.Content.ReadAsStringAsync();
        logger.LogError("Error uploading notebook: {Error}", errorMessage);
        return new ExecuteCommandResult()
        {
            Success = false,
            ErrorMessage = notebookData
        };
    }

    private static HttpClient CreateHttpClient(IResourceBuilder<ContainerResource> builder, string jupyterToken)
    {
        HttpClient? httpclient = null;
        try
        {
            var url = builder.Resource.GetEndpoints().First().Url;
            httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri(url);

            httpclient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("token", jupyterToken);
            return httpclient;
        }
        catch
        {
            httpclient?.Dispose();
            throw;
        }
    }

    private static async Task<ExecuteCommandResult> OnDownloadNotebookCommandAsync(
        IResourceBuilder<ContainerResource> builder,
        ExecuteCommandContext context, string jupyterToken, string jupyterUrl)
    {
        using var httpclient = CreateHttpClient(builder, jupyterToken);
        var queryParams = new Dictionary<string, string>()
        {
            { "type", "file" },
            { "format", "text" },
            { "content", "1" }
        };
        var requestUri = QueryHelpers.AddQueryString($"/api/contents/{NotebookFilename}", queryString: queryParams!);
        var result = await httpclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

        if (!result.IsSuccessStatusCode)
        {
            var errorMessage = await result.Content.ReadAsStringAsync();
            return new ExecuteCommandResult()
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        var noteBookContent = await result.Content.ReadAsStringAsync();
        var destination = Path.Combine(Directory.GetCurrentDirectory(), "Notebooks", NotebookFilename);

        // only need the content property from the response
        var rootNode = JsonNode.Parse(noteBookContent)!;
        var contentNode = rootNode!["content"]!;
        var cleanedUp = contentNode.ToString();

        await File.WriteAllTextAsync(destination, cleanedUp);
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

    [Obsolete("Obsolete")]
    public static IResourceBuilder<ContainerResource> WithDownloadNoteBookCommand(
        this IResourceBuilder<ContainerResource> builder, string jupyterToken, string jupyterUrl)
    {
        builder.WithCommand(
            name: "download-notebook",
            displayName: "Download Notebook",
            executeCommand: context => OnDownloadNotebookCommandAsync(builder, context, jupyterToken, jupyterUrl),
            updateState: OnUpdateResourceState,
            iconName: "ArrowDownload",
            iconVariant: IconVariant.Filled);

        return builder;
    }

    private static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }
}