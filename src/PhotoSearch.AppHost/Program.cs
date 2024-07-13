using System;
using System.Text.RegularExpressions;
using Aspire.Hosting;
using PhotoSearch.Ollama;

var builder = DistributedApplication.CreateBuilder(args);
var dockerHostValue = Environment.GetEnvironmentVariable("DOCKER_HOST");
var dockerHost = string.Empty;

if(!string.IsNullOrEmpty(dockerHostValue))
{
    string pattern = @"(\d{1,3}\.){3}\d{1,3}";
    Match match = Regex.Match(dockerHostValue, pattern);
    if (match.Success)
    {
        dockerHost = match.Value;
    }
}


var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
var rmqPassword = builder.AddParameter("rmqPassword", secret: true);
var pgUsername = builder.AddParameter("pgUsername", secret: true);
var pgPassword = builder.AddParameter("pgPassword", secret: true);

var ollama = builder.AddOllama(hostIpAddress: dockerHost);

var messaging = builder.AddRabbitMQ("messaging", rmqUsername, rmqPassword,5672)
    .WithManagementPlugin()
    .WithContainerRuntimeArgs("-p", $"0.0.0.0:5672:5672")
    .WithContainerRuntimeArgs("-p", $"0.0.0.0:15672:15672");

var postgres = builder.AddPostgres("postgres", pgUsername, pgPassword, 5432)
    .WithDataVolume("postgres", false)
    .WithVolume("pgadmin", "/var/lib/pgadmin")
    .WithContainerRuntimeArgs("-p", $"0.0.0.0:5432:5432");
var postgresDb = postgres.AddDatabase("postgresdb");
 
var apiService = builder.AddProject<Projects.PhotoSearch_API>("apiservice")
    .WithReference(messaging)
    .WithReference(ollama)
    .WithReference(postgresDb);

var backgroundService = builder.AddProject<Projects.PhotoSearch_Worker>("backgroundservice")
    .WithReference(messaging)
    .WithReference(ollama)
    .WithReference(postgresDb);

if (!string.IsNullOrWhiteSpace(dockerHost))
{
    var postgresqlConnection =
        $"Host={dockerHost};Port=5432;Username={pgUsername.Resource.Value};Password={pgPassword.Resource.Value};Database={postgresDb.Resource.Name}";
    var rabbitMqConnectionString =
        $"amqp://{rmqUsername.Resource.Value}:{rmqPassword.Resource.Value}@{dockerHost}:5672";
    
    backgroundService
        .WithEnvironment("ConnectionStrings__postgresdb", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
    apiService
        .WithEnvironment("ConnectionStrings__postgresdb", postgresqlConnection)
        .WithEnvironment("ConnectionStrings__messaging", rabbitMqConnectionString);
}

builder.Build().Run();