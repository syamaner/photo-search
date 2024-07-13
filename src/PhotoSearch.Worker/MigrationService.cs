using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using OpenTelemetry.Trace;
using PhotoSearch.Data;

namespace PhotoSearch.Worker;

public class MigrationService(PhotoSearchContext dbContext): IMigrationService
{
    private const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var attempt = 0;
        const int maxAttempts = 10;
        var dbExists = false;
        while (!await dbContext.Database.CanConnectAsync(cancellationToken) && attempt++ <= maxAttempts && !dbExists)
        {
            await Task.Delay(600, cancellationToken);
            dbExists = await EnsureDatabaseAsync(dbContext, cancellationToken);
        }

        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);

        try
        {
            await EnsureDatabaseAsync(dbContext, cancellationToken);
            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            throw;
        }
    }
 
    private static async Task<bool> EnsureDatabaseAsync(PhotoSearchContext dbContext, CancellationToken cancellationToken)
    {
        try
        {
            var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                if (!await dbCreator.ExistsAsync(cancellationToken))
                {
                    await dbCreator.CreateAsync(cancellationToken);
                }
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task RunMigrationAsync(PhotoSearchContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(PhotoSearchContext dbContext, CancellationToken cancellationToken)
    {
        // SupportTicket firstTicket = new()
        // {
        //     Title = "Test Ticket",
        //     Description = "Default ticket, please ignore!",
        //     Completed = true
        // };
        //
        // var strategy = dbContext.Database.CreateExecutionStrategy();
        // await strategy.ExecuteAsync(async () =>
        // {
        //     // Seed the database
        //     await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        //     await dbContext.Tickets.AddAsync(firstTicket, cancellationToken);
        //     await dbContext.SaveChangesAsync(cancellationToken);
        //     await transaction.CommitAsync(cancellationToken);
        // });
        await Task.CompletedTask;
    }
}