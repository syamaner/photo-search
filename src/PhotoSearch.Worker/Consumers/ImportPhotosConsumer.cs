using MassTransit;
using PhotoSearch.Common.Contracts;

namespace PhotoSearch.Worker.Consumers;

public class ImportPhotosConsumer: IConsumer<ImportPhotos>
{
    public async Task Consume(ConsumeContext<ImportPhotos> context)
    {
        Console.WriteLine($"Importing photos... {context.Message.Directory} ");
        await Task.Delay(1000);
    }
}