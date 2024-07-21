using System;
using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;
using FizzyLogic.Aspire.Python.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();

var ollamaContainer = builder.AddOllama(hostIpAddress: dockerHost, modelName: "llava-phi3",
    useGpu: enableNvidiaDocker);

var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
var rmqPassword = builder.AddParameter("rmqPassword", secret: true);
var messaging = builder.AddRabbitMQ("messaging", rmqUsername, rmqPassword, 5672)
    .WithManagementPlugin();

var pgUsername = builder.AddParameter("pgUsername", secret: true);
var pgPassword = builder.AddParameter("pgPassword", secret: true);
var postgresContainer = builder.AddPostgres("postgres", pgUsername, pgPassword, 5432)
    .WithDataVolume("postgres", false);
var postgresDb = postgresContainer.AddDatabase("photo-db");
var pgAdminContainer = builder.AddContainer("pgadmin", "dpage/pgadmin4")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "a@a.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", pgPassword.Resource.Value)
    .WithReference(postgresDb)
    .WithVolume("pgadmin-data", "/var/lib/pgadmin").WithEndpoint(name: "pgadmin", port: 8080, targetPort:80,scheme:"http");

var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice")
    .WithReference(messaging)
    .WithReference(ollamaContainer)
    .WithReference(postgresDb);

var flaskAppFlorenceApi = builder.AddFlaskProjectWithVirtualEnvironment("florence2api", 
    "../PhotoSearch.Florence2.API/src")
    .WithEnvironment("FLORENCE_MODEL",Environment.GetEnvironmentVariable("FLORENCE_MODEL"))
    .WithEnvironment("PYTHONUNBUFFERED","0");

var backgroundWorker = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(messaging)
    .WithReference(ollamaContainer)
    .WithReference(postgresDb)
    .WithReference(flaskAppFlorenceApi);

// If we are using a Docker host that is not our developer machine, there are additional steps.
if (!string.IsNullOrWhiteSpace(dockerHost))
{
    const string potsgreSqlPort = "5432";
    const string rabbitMqPort = "5672";
    
    // Ensure when running in docker, the services are accessible from local network using Docker host machine ip address.
    messaging.WithContainerRuntimeArgs("-p", $"0.0.0.0:{rabbitMqPort}:{rabbitMqPort}")
        .WithContainerRuntimeArgs("-p", $"0.0.0.0:15672:15672");
    
    postgresContainer.WithContainerRuntimeArgs("-p", $"0.0.0.0:{potsgreSqlPort}:{potsgreSqlPort}");
    pgAdminContainer.WithContainerRuntimeArgs("-p", $"0.0.0.0:8008:80");
    
    // Ensure connection string for containers does not show localhost. Inject the remote Docker host IP instead.
    var postgresqlConnection =
        $"Host={dockerHost};Port={potsgreSqlPort};Username={pgUsername.Resource.Value};Password={pgPassword.Resource.Value};Database={postgresDb.Resource.Name}";
    var rabbitMqConnectionString =
        $"amqp://{rmqUsername.Resource.Value}:{rmqPassword.Resource.Value}@{dockerHost}:{rabbitMqPort}";

    backgroundWorker
        .WithEnvironment("ConnectionStrings__photo-db", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
    apiService
        .WithEnvironment("ConnectionStrings__photo-db", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
}

builder.Build().Run();