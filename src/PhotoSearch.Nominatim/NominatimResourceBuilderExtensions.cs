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
        
        var nominatimResourceBuilder = builder.AddResource(nominatimResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = "mediagis/nominatim", Tag = imageTag })
            .PublishAsContainer()
            .WithEnvironment("PBF_URL", mapUrl)
            .WithEnvironment("IMPORT_WIKIPEDIA", "false")
            .WithEnvironment("IMPORT_GB_POSTCODES", importUkPostcodes ? "true" : "false")
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

        builder.Services.TryAddLifecycleHook<NominatimResourceLifecycleHook>();
        return nominatimResourceBuilder;
    } 
    public static IResourceBuilder<NominatimResource> WithPersistence(this IResourceBuilder<NominatimResource> builder,
        string nominatimDataVolumeName="nominatim-data-sw",
        string nominatimFlatVolumeName = "nominatim-flat-node-sw",
        string nominatimPostgresqlVolumeName = "nominatim-postgres-sw")
    {
        return builder
                .WithVolume(nominatimDataVolumeName, "/nominatim/data")
                .WithVolume(nominatimFlatVolumeName, "/nominatim/flatnode")
                .WithVolume(nominatimPostgresqlVolumeName, "/var/lib/postgresql/14/main")
                //.WithBindMount("./ddd","/dev/shm")
                .WithContainerRuntimeArgs("--shm-size=32g")
                .WithContainerRuntimeArgs("--mount=type=tmpfs,target=/dev/shm");
            ;
    }
}