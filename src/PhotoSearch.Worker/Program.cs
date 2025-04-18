using System.ClientModel;
using System.ClientModel.Primitives;
using MongoDB.Driver;
using OpenAI;
using PhotoSearch.Common;
using PhotoSearch.Data.Models;
using PhotoSearch.ServiceDefaults;
using PhotoSearch.Worker;
using PhotoSearch.Worker.Clients;
using PhotoSearch.Worker.Consumers;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.AddKeyedOpenAIClient(Constants.OpeanAiConnectionName);
builder.Services.AddKeyedSingleton<OpenAIClient>(Constants.OllamaConnectionStringName, (sp, _) => {
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(Constants.OllamaConnectionStringName);
    return new OpenAIClient(new ApiKeyCredential("key_not_required"), new OpenAIClientOptions()
    {
        Endpoint = new Uri(connectionString + "/v1"),
        NetworkTimeout = TimeSpan.FromSeconds(25),
        RetryPolicy = new ClientRetryPolicy(6)
    });
});

builder.Services.AddTransient<IPhotoImporter, PhotoImporter>();
builder.Services.AddTransient<IPhotoSummaryEvaluator, OpenAiPhotoSummaryEvaluationClient>();

builder.Services.AddSingleton<IPhotoSummaryClient, OllamaPhotoSummaryClient>();
builder.Services.AddSingleton<IPhotoSummaryClient, OpenAiPhotoSummaryClient>();

builder.Services.AddHttpClient<IReverseGeocoder, NominatimReverseGeocoder>((sp, httpClient) =>
{
    var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString("Nominatim");
    httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en"));
    httpClient.BaseAddress = new Uri(connectionString!);
});

builder.AddRabbitMQClient("messaging");
builder.AddMongoDBClient("photo-search");

builder.Services.AddScoped<IMongoCollection<Photo>>(x =>
{
    var mongoClient = x.GetRequiredService<IMongoClient>();
    const string collectionName = "photos";
    var db = mongoClient.GetDatabase("photo-search");
    db.CreateCollection(collectionName);
    return db.GetCollection<Photo>(collectionName); 
});

builder.AddMasstransit(configurator =>
{
    configurator.AddConsumer<ImportPhotosConsumer>();
    configurator.AddConsumer<SummarisePhotosConsumer>();
    configurator.AddConsumer<BatchSummarisePhotosConsumer>();
    configurator.AddConsumer<EvaluateModelPerformanceConsumer>();
});

var host = builder.Build();
host.Run();