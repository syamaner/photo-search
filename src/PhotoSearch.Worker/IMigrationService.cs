namespace PhotoSearch.Worker;

public interface IMigrationService
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}