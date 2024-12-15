using System;
using System.Threading;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OllamaSharp;
using OllamaSharp.Models;

namespace PhotoSearch.AppHost.DashboardCommands;

internal static class MongoDbCommands 
{
    public static IResourceBuilder<MongoDBDatabaseResource> WithResetDatabaseCommand(
        this IResourceBuilder<MongoDBDatabaseResource> builder)
    {
        builder.WithCommand(
            name: "reset-database",
            displayName: "Reset Database",
            executeCommand: context => OnResetDatabaseCommandAsync(builder, context),
            updateState: OnUpdateResourceState,
            iconName: "ArrowDelete",
            iconVariant: IconVariant.Filled);

        return builder;
    }
        
    private static async Task<ExecuteCommandResult> OnResetDatabaseCommandAsync(
        IResourceBuilder<MongoDBDatabaseResource> builder,
        ExecuteCommandContext context)
    {        
        var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var notificationService = context.ServiceProvider.GetRequiredService<ResourceNotificationService>();
               
        var mongoDbConnectionString = await builder.Resource.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
        
        // Reset the database
        var mongoClient = new MongoClient(mongoDbConnectionString);
        const string databaseName = "photo-search";
        var database = mongoClient.GetDatabase(databaseName);
        await database.DropCollectionAsync("photos");

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