using Npgsql;
using OllamaSharp;
using PhotoSearch.Common;
using PhotoSearch.Data;
using PhotoSearch.ServiceDefaults;
using PhotoSearch.Worker;
using PhotoSearch.Worker.Clients;
using PhotoSearch.Worker.Consumers;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<IMigrationService, MigrationService>();
builder.Services.AddSingleton<IPhotoSummaryClient, OllamaPhotoSummaryClient>();
builder.Services.AddTransient<IPhotoImporter, PhotoImporter>();

//builder.Services.AddTransient<IPhotoSummaryClient, Florence2PhotoSummaryClient>();

builder.Services.AddHttpClient<IPhotoSummaryClient, Florence2PhotoSummaryClient>((sp, httpClient) =>
{
    var cs = sp.GetRequiredService<IConfiguration>().GetChildren();
//private static readonly string? FlorenceUrl = Environment.GetEnvironmentVariable("services__florence2api__http__0");
    var connectionString = sp.GetRequiredService<IConfiguration>().GetSection("services:florence2api:http:0");
    httpClient.BaseAddress = new Uri(connectionString.Value!);
    
});
builder.Services.AddSingleton<IOllamaApiClient>(sp =>
{
    var lamaConnectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Ollama");
    return new OllamaApiClient(new Uri(lamaConnectionString!));
});

builder.Services.AddHttpClient<IReverseGeocoder, NominatimReverseGeocoder>((sp, httpClient) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Nominatim");
    httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
    httpClient.BaseAddress = new Uri(connectionString!);
});

builder.AddRabbitMQClient("messaging");
builder.AddNpgsqlDbContext<PhotoSearchContext>("photo-db");
builder.AddMasstransit(configurator =>
{
    configurator.AddConsumer<ImportPhotosConsumer>();
    configurator.AddConsumer<SummarisePhotosConsumer>();
});

var host = builder.Build();
host.Run();