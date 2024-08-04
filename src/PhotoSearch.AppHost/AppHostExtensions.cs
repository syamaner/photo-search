using System.Linq;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.AppHost;

public static class AppHostExtensions
{
    public static IResourceBuilder<PostgresServerResource> AddPostgreSql(this IDistributedApplicationBuilder builder,
        string dbName, int publicPort, string? host)
    {
        var pgUsername = builder.AddParameter("pgUsername", secret: true);
        var pgPassword = builder.AddParameter("pgPassword", secret: true);
        var postgresContainer =
            builder
                .AddPostgres("postgres", pgUsername, pgPassword, port: publicPort)
                .WithDataVolume("postgres", false);
        
        if (string.IsNullOrWhiteSpace(host)) return postgresContainer;
        //
        postgresContainer
            .WithContainerRuntimeArgs("--add-host", "host.docker.internal:host-gateway");

        if(postgresContainer.Resource.Annotations.FirstOrDefault(x => x is EndpointAnnotation) is not EndpointAnnotation endpointAnnotation) return postgresContainer;
        
        endpointAnnotation!.IsProxied = false; 
        endpointAnnotation.IsExternal = true;
        return postgresContainer;
    }
    
    public static  IResourceBuilder<ContainerResource>  AddPgAdmin(this IDistributedApplicationBuilder builder, 
        IResourceBuilder<PostgresServerResource> ogResource, 
        int publicPort,
        string pgAdminLogin = "a@a.com")
    {
        var pgAdminContainer = builder.AddContainer("pgadmin", "dpage/pgadmin4")
            .WithEnvironment("PGADMIN_DEFAULT_EMAIL", pgAdminLogin)
            .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", ogResource.Resource.PasswordParameter.Value)
            .WithEnvironment("PGADMIN_LISTEN_PORT", publicPort.ToString())
            .WithVolume("pgadmin-data", "/var/lib/pgadmin")
            .WithHttpEndpoint(publicPort, publicPort, "http", isProxied: false)
            .WithReference(ogResource);

        pgAdminContainer
            .WithContainerRuntimeArgs("--add-host", "host.docker.internal:host-gateway");
        return pgAdminContainer;
    }
    
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