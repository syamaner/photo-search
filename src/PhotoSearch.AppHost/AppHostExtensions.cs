using System;
using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.AppHost.DashboardCommands;

namespace PhotoSearch.AppHost;

public static class AppHostExtensions
{
    public static IResourceBuilder<RabbitMQServerResource> AddRabbitMq(this IDistributedApplicationBuilder builder,
        string name, bool isRemoteDockerHost, int ampqPort = 5672, int adminPort = 15672)
    {
        var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
        var rmqPassword = builder.AddParameter("rmqPassword", secret: true);
        var rabbitMq = builder.AddRabbitMQ(name, rmqUsername, rmqPassword, ampqPort)
            .WithImageTag("3-management") 
            .WithHttpEndpoint(adminPort, adminPort, "http")
            .WithLifetime(ContainerLifetime.Session)
            .WithExternalHttpEndpoints();

        if (!isRemoteDockerHost) return rabbitMq;
        foreach (var annotation in rabbitMq.Resource.Annotations.Where(x =>
                     x is EndpointAnnotation))
        {
            var endpointAnnotation = (EndpointAnnotation)annotation;
            endpointAnnotation.IsProxied = false;
        }
        
        return rabbitMq;
    }

    [Obsolete("Obsolete")]
    public static IResourceBuilder<ContainerResource> AddJupyter(this IDistributedApplicationBuilder builder,
        string name, bool isRemoteDockerHost, string token="secret", int port = 8888)
    {
        var image = "quay.io/jupyter/pytorch-notebook";
        var tag = isRemoteDockerHost ? "cuda12-python-3.11.8" : "latest";
        var jupyter = builder.AddContainer(name, image)
            .WithImageTag(tag)  
            .WithLifetime(ContainerLifetime.Session)
            .WithContainerRuntimeArgs("--entrypoint=start-notebook.sh")
            .WithArgs($"--NotebookApp.token={token}")
            .WithHttpEndpoint(port, port, "http", "jupyter",isProxied:false)
            .WithUploadNoteBookCommand(token, "http://localhost:8888")
            .WithDownloadNoteBookCommand(token, "http://localhost:8888")
            .WithExternalHttpEndpoints(); 
        
        if(isRemoteDockerHost)
            jupyter.WithContainerRuntimeArgs("--gpus=all");
      
        return jupyter;

    }

    public static IResourceBuilder<MongoDBServerResource> AddMongo(this IDistributedApplicationBuilder builder,
        string name, bool isRemoteDockerHost, int port = 27017)
    {

        var mongoDb = builder.AddMongoDB(name, port:port)
            .WithDataVolume("mongo-photo-search", false);

        if (!isRemoteDockerHost) return mongoDb;


        foreach (var annotation in mongoDb.Resource.Annotations.Where(x =>
                     x is EndpointAnnotation))
        {
            var endpointAnnotation = (EndpointAnnotation)annotation;
            endpointAnnotation.IsProxied = false;
            Console.WriteLine($"Setting {name} to not be proxied");
        }
        
        return mongoDb;
    }
    
    
    public static IResourceBuilder<QdrantServerResource> AddQdrant1(this IDistributedApplicationBuilder builder,
        string name, bool isRemoteDockerHost)
    {

        var vectorStore = builder.AddQdrant(Constants.ConnectionStringNames.Qdrant)
            .WithImageTag("v1.13.0-unprivileged")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithDataVolume("qdrant", false);

        if (!isRemoteDockerHost) return vectorStore;


        foreach (var annotation in vectorStore.Resource.Annotations.Where(x =>
                     x is EndpointAnnotation))
        {
            var endpointAnnotation = (EndpointAnnotation)annotation;
            endpointAnnotation.IsProxied = false;
            Console.WriteLine($"Setting {name} to not be proxied");
        }
        
        return vectorStore;
    }
}