using System;
using System.Collections.Generic;
using System.Linq;
using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;
using PhotoSearch.AppHost.WaitFor; 
using PhotoSearch.Nominatim;
using PhotoSearch.MapTileServer;

var portMappings = new Dictionary<string,PortMap>
{
    { "MapTileService", new PortMap(8080, 80, "MapTileService")},
    { "RabbitMQ", new PortMap(5672, 5672, "RabbitMQ")},
    { "RabbitMQManagement", new PortMap(15672, 15672, "RabbitMQManagement")},
    { "Nominatim", new PortMap(8180, 8080, "Nominatim")}, 
    { "Ollama",  new PortMap(11438, 11434, "Ollama")},
    { "MongoDB",  new PortMap(27019, 27019, "MongoDB")},
    { "Florence3Api",  new PortMap(8111, 8111, "Florence3Api", false)},
    { "FEPort",  new PortMap(3333, 3333, "FEPort", false)}
};

var builder = DistributedApplication.CreateBuilder(args);
var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();
var ollamaVisionModel =  Environment.GetEnvironmentVariable("OLLAMA_MODEL");
var mapDownloadUrl = Environment.GetEnvironmentVariable("NOMINATIM_MAP_URL") ?? "http://download.geofabrik.de/europe/switzerland-latest.osm.pbf";

var mongo = builder.AddMongo("mongo",
    !string.IsNullOrWhiteSpace(dockerHost), port: portMappings["MongoDB"].PublicPort);
var mongodb = mongo.AddDatabase("photo-search");

var osmTileService =builder.AddMapTileServer(!string.IsNullOrWhiteSpace(dockerHost),
    hostPort: portMappings["MapTileService"].PublicPort, containerPort: portMappings["MapTileService"].PrivatePort);

var ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: ollamaVisionModel!,
        useGpu: enableNvidiaDocker, hostPort: portMappings["Ollama"].PublicPort,
        ollamaContainerPort: portMappings["Ollama"].PrivatePort)
    .WithHealthCheck();

var nominatimContainer =
    builder.AddNominatim(name: "Nominatim",
            isRemoteDockerHost: !string.IsNullOrWhiteSpace(dockerHost),
            mapUrl: mapDownloadUrl!,
            hostPort: portMappings["Nominatim"].PublicPort, 
            containerPort: portMappings["Nominatim"].PrivatePort)
        .WithPersistence()
        .WithHealthCheck();

var messaging =
    builder.AddRabbitMq("messaging", !string.IsNullOrWhiteSpace(dockerHost), ampqPort: portMappings["RabbitMQ"].PublicPort,
            adminPort: portMappings["RabbitMQManagement"].PublicPort)
        .WithHealthCheck();

var florence3Api = builder
    .AddPythonProject("florence2api",
        "../PhotoSearch.Florence2.API/src", "main.py")
    .WithEndpoint(targetPort: portMappings["Florence3Api"].PublicPort, scheme: "http", env: "PORT")
    .WithEnvironment("FLORENCE_MODEL", Environment.GetEnvironmentVariable("FLORENCE_MODEL"))
    .WithEnvironment("PYTHONUNBUFFERED", "0")
    .WithEnvironment("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice") 
    .WithReference(ollamaContainer)
    .WaitFor(messaging)
    .WithReference(mongodb)
    .WithReference(messaging);

var backgroundWorker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(ollamaContainer)
    .WithReference(florence3Api)
    .WithReference(nominatimContainer)
    .WithReference(messaging)
    .WithReference(mongodb)
    .WaitFor(ollamaContainer)
    .WaitFor(nominatimContainer)
    .WaitFor(messaging);

builder.AddNpmApp("stencil", "../photosearch-frontend")
    .WithReference(apiService)
    .WithReference(osmTileService)
    .WithHttpEndpoint(port: portMappings["FEPort"].PublicPort, 
        targetPort: portMappings["FEPort"].PrivatePort, 
        env: "PORT", 
        isProxied:false)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// add ssh_user and ssh_key_file (path to the key file) for ser secrets.
using var sshUtility = new SShUtility(dockerHost, builder.Configuration["ssh_user"]!,  builder.Configuration["ssh_key_file"]!);

if (!string.IsNullOrWhiteSpace(dockerHost))
{
    // Forwards the ports to the docker host machine
    sshUtility.Connect();
    foreach (var portMapping in portMappings.Values.Where(x => x.PortForward))
    {
        Console.WriteLine($"Forwarding port {portMapping.PublicPort} to {portMapping.PublicPort}");
        sshUtility.AddForwardedPort(portMapping.PublicPort, portMapping.PublicPort);
    } 
}

builder.Build().Run();

public record PortMap(int PublicPort, int PrivatePort, string Name, bool PortForward = true);