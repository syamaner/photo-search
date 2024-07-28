using System;
using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;
using FizzyLogic.Aspire.Python.Hosting;
using PhotoSearch.AppHost.WaitFor;
using PhotoSearch.Nominatim;

var builder = DistributedApplication.CreateBuilder(args);

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();
var ollamaVisionModel =  Environment.GetEnvironmentVariable("VISION_MODEL");
var mapUrl = Environment.GetEnvironmentVariable("NOMINATIM_MAP_URL") ?? "http://download.geofabrik.de/europe/switzerland-latest.osm.pbf";
var dbName = "photo-db";
 
var ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: ollamaVisionModel!,
    useGpu: enableNvidiaDocker)
    .WithHealthCheck();

var nominatimContainer =
    builder.AddNominatim(name:"Nominatim", hostIpAddress: dockerHost, mapUrl: mapUrl!, hostPort: 8180, imageTag: "4.4")
        .WithPersistence()
        .WithHealthCheck();

var postgresContainer = builder.AddPostgreSql(dbName, 5432, dockerHost);
var postgresDb = postgresContainer.AddDatabase(dbName);

var messaging =
    builder.AddRabbitMq("messaging", dockerHost, 5672)
        .WithHealthCheck(dockerHost);

var flaskAppFlorenceApi = builder.AddFlaskProjectWithVirtualEnvironment("florence2api", 
        "../PhotoSearch.Florence2.API/src")
    .WithEnvironment("FLORENCE_MODEL",Environment.GetEnvironmentVariable("FLORENCE_MODEL"))
    .WithEnvironment("PYTHONUNBUFFERED","0");

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice") 
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WaitFor(messaging)
    .WithReference(messaging);

var backgroundWorker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WithReference(flaskAppFlorenceApi)
    .WithReference(nominatimContainer)
    .WaitFor(ollamaContainer)
    .WaitFor(nominatimContainer)
    .WaitFor(messaging)
    .WithReference(messaging);

builder.AddNpmApp("stencil", "../photosearch-frontend")
    .WithReference(apiService)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


if (!string.IsNullOrWhiteSpace(dockerHost))
{
    builder.UpdateRabbitmqConnectionString(backgroundWorker, messaging, dockerHost);
    builder.UpdateRabbitmqConnectionString(apiService, messaging, dockerHost);
}


builder.Build().Run();