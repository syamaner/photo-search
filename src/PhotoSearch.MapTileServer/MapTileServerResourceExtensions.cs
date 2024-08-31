using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

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
        var nominatimResource = new MapTileServerResource(name, hostPort!.Value);

        builder.Services.TryAddLifecycleHook<MapTileServerResourceLifecycleHook>();

        var resourceBuilder = builder.AddResource(nominatimResource)
            .WithImage("syamaner/osm-tile-server")
            .WithImageTag("2.3.0")
            .WithEnvironment("DOWNLOAD_PBF", mapUrl)
            .WithEnvironment("DOWNLOAD_POLY", polygonUrl)
            .WithEnvironment("ALLOW_CORS", "enabled")
            .WithVolume("map-tile-db", "/data/database")
            .WithEndpoint(hostPort, containerPort, "http",
                name:"http",
                isProxied: false)
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