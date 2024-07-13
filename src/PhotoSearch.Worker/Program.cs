using MassTransit;
using PhotoSearch.Data;
using PhotoSearch.Worker;
using PhotoSearch.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IMigrationService, MigrationService>();
builder.AddRabbitMQClient("messaging");
builder.AddNpgsqlDbContext<PhotoSearchContext>("postgresdb");

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddHealthChecks();
    x.AddConsumer<ImportPhotosConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        var configuration = context.GetRequiredService<IConfiguration>();
        var host = configuration.GetConnectionString("messaging");
        cfg.Host(host);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
