using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoSearch.MapTileServer;

public static class MapTileServerResourceExtensions
{
    public static IResourceBuilder<MapTileServerResource> AddMapTileServer(this IDistributedApplicationBuilder builder,
        bool isRemoteDockerHost,
        string mapUrl= "https://download.geofabrik.de/europe/united-kingdom/england/greater-london-latest.osm.pbf",
        string polygonUrl="https://download.geofabrik.de/europe/united-kingdom/england/greater-london.poly",
        string name = "OSMMapTileServer",
        int? hostPort = 8080,
        int containerPort = 80)
    {
        var mapTileServerResource = new MapTileServerResource(name, hostPort!.Value);

        builder.Services.AddHealthChecks().AddTypeActivatedCheck<MapTileServerHealthCheck>("maptile-healthcheck",mapTileServerResource.ConnectionStringExpression.ValueExpression);

        
        var resourceBuilder = builder.AddResource(mapTileServerResource)
            .WithImage("syamaner/osm-tile-server")
            .WithImageTag("2.3.0")
            .WithContainerName(name)
            .WithEnvironment("DOWNLOAD_PBF", mapUrl)
            .WithEnvironment("DOWNLOAD_POLY", polygonUrl)
            .WithEnvironment("ALLOW_CORS", "enabled")
            .WithVolume("map-tile-db", "/data/database")
            .WithHealthCheck("maptile-healthcheck")
            .WithEndpoint(hostPort, containerPort, "http",
                name:"http",
                isProxied: false)
            .WithLifetime(ContainerLifetime.Persistent)
            .PublishAsContainer();

 
        if (!isRemoteDockerHost) return resourceBuilder;

        foreach (var resourceAnnotation in resourceBuilder.Resource.Annotations.Where(x =>
                     x is EndpointAnnotation))
        {
            var endpointAnnotation = (EndpointAnnotation)resourceAnnotation;
            endpointAnnotation.IsProxied = false;
        }

        return resourceBuilder;
    }

}