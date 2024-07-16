using Aspire.Hosting;
using PhotoSearch.AppHost;
using PhotoSearch.Ollama;

var builder = DistributedApplication.CreateBuilder(args);

var dockerHost = StartupHelper.GetDockerHostValue();
var enableNvidiaDocker = StartupHelper.NvidiaDockerEnabled();

var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
var rmqPassword = builder.AddParameter("rmqPassword", secret: true);
var messaging = builder.AddRabbitMQ("messaging", rmqUsername, rmqPassword, 5672)
    .WithManagementPlugin();

var pgUsername = builder.AddParameter("pgUsername", secret: true);
var pgPassword = builder.AddParameter("pgPassword", secret: true);
var postgres = builder.AddPostgres("postgres", pgUsername, pgPassword, 5432)
    .WithDataVolume("postgres", false);
var pgAdmin = builder.AddContainer("pgadmin", "dpage/pgadmin4")
    .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "a@a.com")
    .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", pgPassword.Resource.Value)
    .WithVolume("pgadmin-data", "/var/lib/pgadmin").WithEndpoint(name: "pgadmin", port: 8080, targetPort:80,scheme:"http");
var postgresDb = postgres.AddDatabase("photo-db");

var ollama = builder.AddOllama(hostIpAddress: dockerHost, modelName: "llava:7b",
    useGpu: enableNvidiaDocker);


var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice")
    .WithReference(messaging)
    .WithReference(ollama)
    .WithReference(postgresDb);

var backgroundService = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(messaging)
    .WithReference(ollama)
    .WithReference(postgresDb);

// If we are using a Docker host that is not our developer machine, there are additional steps.
if (!string.IsNullOrWhiteSpace(dockerHost))
{
    // Ensure when running in docker, the services are accessible from local network using Docker host machine ip address.
    messaging.WithContainerRuntimeArgs("-p", $"0.0.0.0:5672:5672")
        .WithContainerRuntimeArgs("-p", $"0.0.0.0:15672:15672");
    postgres.WithContainerRuntimeArgs("-p", $"0.0.0.0:5432:5432");
    pgAdmin.WithContainerRuntimeArgs("-p", $"0.0.0.0:8008:80");
    
    // Ensure connection string for containers does not sgow localhost. Inject the Docker host IP instead.
    var postgresqlConnection =
        $"Host={dockerHost};Port=5432;Username={pgUsername.Resource.Value};Password={pgPassword.Resource.Value};Database={postgresDb.Resource.Name}";
    var rabbitMqConnectionString =
        $"amqp://{rmqUsername.Resource.Value}:{rmqPassword.Resource.Value}@{dockerHost}:5672";
    backgroundService
        .WithEnvironment("ConnectionStrings__photo-db", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
    apiService
        .WithEnvironment("ConnectionStrings__photo-db", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
}

builder.Build().Run();