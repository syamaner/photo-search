using MassTransit;
using PhotoSearch.Common.Contracts;

namespace PhotoSearch.Worker;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        {
            var dbMigrator = scope.ServiceProvider.GetRequiredService<IMigrationService>();
            await dbMigrator.ExecuteAsync(stoppingToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Applied migration at: {time}", DateTimeOffset.Now);
            }

            var bus = serviceProvider.GetService<IBus>();
            while (stoppingToken.IsCancellationRequested == false)
            {
                await bus!.Publish(new ImportPhotos("C:\\Photos"), stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}