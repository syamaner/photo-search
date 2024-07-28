using System;
using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;
using FizzyLogic.Aspire.Python.Hosting;
using PhotoSearch.Nominatim;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();
var ollamaVisionModel = Environment.GetEnvironmentVariable("VISION_MODEL");
var mapUrl =Environment.GetEnvironmentVariable("NOMINATIM_MAP_URL") ?? "http://download.geofabrik.de/europe/switzerland-latest.osm.pbf";
var dbName = "photo-db";
 
var ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: ollamaVisionModel!,
    useGpu: enableNvidiaDocker);
 
var nominatimContainer =
    builder.AddNominatim(hostIpAddress: dockerHost, mapUrl: mapUrl!, hostPort: 8180, imageTag: "4.4");

var postgresContainer = builder.AddPostgreSql(dbName, 5432, dockerHost);
var postgresDb = postgresContainer.AddDatabase(dbName);
 
var flaskAppFlorenceApi = builder.AddFlaskProjectWithVirtualEnvironment("florence2api", 
        "../PhotoSearch.Florence2.API/src")
    .WithEnvironment("FLORENCE_MODEL",Environment.GetEnvironmentVariable("FLORENCE_MODEL"))
    .WithEnvironment("PYTHONUNBUFFERED","0");

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice") 
    .WithReference(ollamaContainer)
    .WithReference(postgresDb);

var backgroundWorker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WithReference(flaskAppFlorenceApi)
    .WithReference(nominatimContainer);

var messaging =
    builder.AddRabbitMq("messaging", dockerHost, 5672,
        apiService, backgroundWorker);
 
builder.Build().Run();