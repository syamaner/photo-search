using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace PhotoSearch.Nominatim;

public static class NominatimResourceBuilderExtensions
{
    private const int ContainerPort = 8080;
    public static IResourceBuilder<NominatimResource> AddNominatim(this IDistributedApplicationBuilder builder,
        string mapUrl,
        string hostIpAddress = "",
        string imageTag = "4.4",
        string name = "Nominatim",
        bool importUkPostcodes = false,
        int? hostPort = 8180)
    {
        var nominatimResource = new NominatimResource(name, mapUrl, hostIpAddress, hostPort.ToString()!);
        builder.Services.TryAddLifecycleHook<NominatimResourceLifecycleHook>();
        
        var nominatimResourceBuilder = builder.AddResource(nominatimResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "mediagis/nominatim", Tag = imageTag })
            .PublishAsContainer()
            .WithVolume("nominatim-data-sw", "/nominatim/data")
            .WithVolume("nominatim-flat-node-sw", "/nominatim/flatnode")
            .WithVolume("nominatim-postgres-sw", "/var/lib/postgresql/14/main")
            .WithEnvironment("PBF_URL", mapUrl)
            .WithEnvironment("IMPORT_WIKIPEDIA", "true")
            .WithContainerRuntimeArgs("--shm-size=8g")
            .WithEnvironment("IMPORT_GB_POSTCODES", importUkPostcodes ? "true" : "false")
  //          .WithEnvironment("UPDATE_MODE","continuous")
           // .WithEnvironment("REPLICATION_URL","https://download.geofabrik.de/europe-updates/")
            .WithExternalHttpEndpoints();
        
        if (!string.IsNullOrWhiteSpace(hostIpAddress))
        {
            nominatimResourceBuilder
                .WithContainerRuntimeArgs("-p", $"0.0.0.0:{hostPort}:{ContainerPort}");
        }
        else
        {
            nominatimResourceBuilder.WithHttpEndpoint(hostPort, ContainerPort);
        }

        return nominatimResourceBuilder;
    }
}