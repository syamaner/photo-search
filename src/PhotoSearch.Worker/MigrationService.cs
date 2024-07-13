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
        while (!await dbContext.Database.CanConnectAsync(cancellationToken))
        {
            await Task.Delay(500, cancellationToken);
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
 
    private static async Task EnsureDatabaseAsync(PhotoSearchContext dbContext, CancellationToken cancellationToken)
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