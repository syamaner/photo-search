using System;
using System.Diagnostics;
using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using HealthChecks.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using PhotoSearch.Ollama;

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
}