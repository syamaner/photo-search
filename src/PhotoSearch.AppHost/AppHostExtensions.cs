using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.AppHost;

public static class AppHostExtensions
{
    public static IResourceBuilder<RabbitMQServerResource> AddRabbitMq(this IDistributedApplicationBuilder builder,
        string name, bool isRemoteDockerHost, int ampqPort = 5672, int adminPort = 15672)
    {
        var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
        var rmqPassword = builder.AddParameter("rmqPassword", secret: true);

        var rabbitMq = builder.AddRabbitMQ(name, rmqUsername, rmqPassword, ampqPort)
            .WithImageTag("3-management");
        
        rabbitMq.WithHttpEndpoint(adminPort, adminPort, "http")
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
}