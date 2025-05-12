using System;
using System.Collections.Generic;
using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.AppHost;
using PhotoSearch.AppHost.DashboardCommands;
using PhotoSearch.Ollama;
using PhotoSearch.Nominatim;
using PhotoSearch.MapTileServer;

// int grpcPort = 6334, int httpPort = 6333
var portMappings = new Dictionary<string, PortMap>
{
    { "MapTileService", new PortMap(8080, 80) },
    { "RabbitMQ", new PortMap(5672, 5672) },
    { "RabbitMQManagement", new PortMap(15672, 15672) },
    { "Nominatim", new PortMap(8180, 8080) },
    { "Ollama", new PortMap(11438, 11434) },
    { "MongoDB", new PortMap(27019, 27019) },
    { "Florence3Api", new PortMap(8111, 8111, false) },
    { "FEPort", new PortMap(3333, 3333, false) },
    { "JupyterPort", new PortMap(8888, 8888, true) },
    { "QdrantGrpcPort", new PortMap(6334, 6334, true) },
    { "QdrantHttpPort", new PortMap(6333, 6333, true) }
};

var builder = DistributedApplication.CreateBuilder(args);


var vectorStoreVectorName = Environment.GetEnvironmentVariable("VECTOR_STORE_VECTOR_NAME") ?? "page_content_vector";

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();
var ollamaVisionModel = Environment.GetEnvironmentVariable("OLLAMA_MODEL");
var mapDownloadUrl = Environment.GetEnvironmentVariable("NOMINATIM_MAP_URL")
                     ?? "https://download.geofabrik.de/europe/switzerland-latest.osm.pbf";

var mongo = builder.AddMongo("mongo",
        !string.IsNullOrWhiteSpace(dockerHost), port: portMappings["MongoDB"].PublicPort)
    .WithLifetime(ContainerLifetime.Session);

var mongodb = mongo.AddDatabase("photo-search"); //.WithResetDatabaseCommand();
var vectorStore = builder.AddQdrant1("qdrant", !string.IsNullOrWhiteSpace(dockerHost));

var osmTileService = builder
    .AddMapTileServer(!string.IsNullOrWhiteSpace(dockerHost),
        hostPort: portMappings["MapTileService"].PublicPort,
        containerPort: portMappings["MapTileService"].PrivatePort);
var ollamaUrl = Environment.GetEnvironmentVariable("OLLAMA_URL");
IResourceBuilder<OllamaResource>? ollamaContainer = null;
if (string.IsNullOrWhiteSpace(ollamaUrl))
{
    ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: ollamaVisionModel!,
            useGpu: enableNvidiaDocker, hostPort: portMappings["Ollama"].PublicPort,
            ollamaContainerPort: portMappings["Ollama"].PrivatePort).WithOllamaDownloadCommand()
        .WithEnvironment("OLLAMA_DEBUG", "1");
}

var nominatimContainer =
    builder.AddNominatim(name: "Nominatim",
            isRemoteDockerHost: !string.IsNullOrWhiteSpace(dockerHost),
            mapUrl: mapDownloadUrl,
            hostPort: portMappings["Nominatim"].PublicPort,
            containerPort: portMappings["Nominatim"].PrivatePort)
        .WithPersistence();

var messaging =
    builder.AddRabbitMq("messaging", !string.IsNullOrWhiteSpace(dockerHost),
        ampqPort: portMappings["RabbitMQ"].PublicPort,
        adminPort: portMappings["RabbitMQManagement"].PublicPort);

var openaiConnection = builder.AddConnectionString("openaiConnection");

var openAIKey = builder.AddParameter("OpenAIKey", secret: true);

var jupyterNotebook = builder.AddJupyter("jupyter", !string.IsNullOrWhiteSpace(dockerHost), "secret",
        portMappings["JupyterPort"].PublicPort)
    .WithReference(mongodb);

#pragma warning disable ASPIREHOSTINGPYTHON001
var pythonapp = builder.AddPythonApp("llama-cpp-host", "../PhotoSearch.LlamaCpp", "run.py")
#pragma warning restore ASPIREHOSTINGPYTHON001
    .WithHttpEndpoint(port: 8045, env: "PORT", isProxied: false)
    .WithExternalHttpEndpoints()
    .WithOtlpExporter();

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice")
    .WithReference(mongodb)
    .WithReference(messaging)
    .WithReference(pythonapp)
    .WithReference(openaiConnection)
    .WithReference(vectorStore)
    .WithSummariseCommand()
    .WithEnvironment("DOTNET_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:21268")
    .WithEnvironment("ModelConfiguration__VectorStoreVectorName", vectorStoreVectorName)
    .WithEnvironment("OpenAIKey", openAIKey.Resource.Value)
    .WaitFor(mongodb)
    .WaitFor(vectorStore)
    .WaitFor(messaging);

var worker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithEnvironment("DOTNET_DASHBOARD_OTLP_ENDPOINT_URL", "http://localhost:21268")
    .WithReference(nominatimContainer)
    .WithReference(mongodb)
    .WithReference(messaging)
    .WithReference(openaiConnection)
    .WithEnvironment("OpenAIKey", openAIKey.Resource.Value)
    .WaitFor(nominatimContainer)
    .WaitFor(mongodb)
    .WaitFor(messaging);

var ui = builder.AddNpmApp("stencil", "../photosearch-frontend")
    .WithReference(apiService)
    .WithReference(osmTileService)
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:16175")
    .WithEnvironment("OTEL_EXPORTER_OTLP_PROTOCOL", "http/protobuf")
    .WithHttpEndpoint(port: portMappings["FEPort"].PublicPort,
        targetPort: portMappings["FEPort"].PrivatePort,
        env: "PORT",
        isProxied: false)
    .WithExternalHttpEndpoints()
    .WaitFor(osmTileService)
    .WaitFor(nominatimContainer)
    .WaitFor(messaging)
    .PublishAsDockerFile();


// add ssh_user and ssh_key_file (path to the key file) for ser secrets.
using var sshUtility = new SShUtility(dockerHost, builder.Configuration["ssh_user"]!,
    builder.Configuration["ssh_key_file"]!);

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

if (ollamaContainer != null)
{
    apiService.WithReference(ollamaContainer)
        .WaitFor(ollamaContainer);
    worker.WithReference(ollamaContainer)
        .WaitFor(ollamaContainer);
    ui
        .WaitFor(ollamaContainer);
}
else
{
    apiService.WithEnvironment("ConnectionStrings__Ollama", ollamaUrl);
    worker.WithEnvironment("ConnectionStrings__Ollama", ollamaUrl);
    ui.WithEnvironment("ConnectionStrings__Ollama", ollamaUrl);
}

builder.Build().Run();

public record PortMap(int PublicPort, int PrivatePort, bool PortForward = true);