using System;
using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;
using PhotoSearch.AppHost.WaitFor;
using PhotoSearch.Nominatim;

var builder = DistributedApplication.CreateBuilder(args);

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();
var ollamaVisionModel =  Environment.GetEnvironmentVariable("OLLAMA_MODEL");
var mapDownloadUrl = Environment.GetEnvironmentVariable("NOMINATIM_MAP_URL") ?? "http://download.geofabrik.de/europe/switzerland-latest.osm.pbf";
var dbName = "photo-db";
uint dbPort = 5432;

var ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: ollamaVisionModel!,
    useGpu: enableNvidiaDocker)
    .WithHealthCheck();

var nominatimContainer =
    builder.AddNominatim(name: "Nominatim",
            isRemoteDockerHost: !string.IsNullOrWhiteSpace(dockerHost),
            mapUrl: mapDownloadUrl!)
        .WithPersistence()
        .WithHealthCheck();

var postgresContainer = builder.AddPostgreSql(dbName, (int)dbPort, dockerHost);
builder.AddPgAdmin(postgresContainer, 8081, dockerHost);
var postgresDb = postgresContainer.AddDatabase(dbName);

var messaging =
    builder.AddRabbitMq("messaging", !string.IsNullOrWhiteSpace(dockerHost), 5672)
        .WithHealthCheck();

var florence3Api = builder.AddPythonProject("florence2api", 
        "../PhotoSearch.Florence2.API/src", "main.py")
    .WithEndpoint(targetPort: 8111, scheme: "http", env: "PORT")
    .WithEnvironment("FLORENCE_MODEL",Environment.GetEnvironmentVariable("FLORENCE_MODEL"))
    .WithEnvironment("PYTHONUNBUFFERED","0")
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT","true");

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice") 
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WaitFor(messaging)
    .WithReference(messaging);

var backgroundWorker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WithReference(florence3Api)
    .WithReference(nominatimContainer)
    .WithReference(messaging)
    .WaitFor(ollamaContainer)
    .WaitFor(nominatimContainer)
    .WaitFor(messaging);

// builder.AddNpmApp("stencil", "../photosearch-frontend")
//     .WithReference(apiService)
//     .WithHttpEndpoint(env: "PORT")
//     .WithExternalHttpEndpoints()
//     .PublishAsDockerFile();

// add ssh_user and ssh_key_file (path to the key file) for ser secrets.
using var sshUtility = new SShUtility(dockerHost, builder.Configuration["ssh_user"]!,  builder.Configuration["ssh_key_file"]!);

if (!string.IsNullOrWhiteSpace(dockerHost))
{
    // Forwards the ports to the docker host machine
    sshUtility.Connect();
    // PgAdmin
    sshUtility.AddForwardedPort(8081, 8081);
    // Postgres
    sshUtility.AddForwardedPort(5432, 5432);
    // RabbitMQ
    sshUtility.AddForwardedPort(5672, 5672);
    // RabbitMQ Management
    sshUtility.AddForwardedPort(15672, 15672);
    // Nominatim
    sshUtility.AddForwardedPort(8180, 8180);
    // Ollama
    sshUtility.AddForwardedPort(11438, 11438);
}

builder.Build().Run();

 