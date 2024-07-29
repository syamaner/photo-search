using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;

namespace PhotoSearch.Nominatim;

public static class NominatimResourceBuilderExtensions
{
    private const int ContainerPort = 8080;
    private const string NominatimImage = "mediagis/nominatim";
    public static IResourceBuilder<NominatimResource> AddNominatim(this IDistributedApplicationBuilder builder,
        string mapUrl,
        string hostIpAddress = "",
        string imageTag = "4.4",
        string name = "Nominatim",
        bool importUkPostcodes = false,
        bool importWikipediaData = false,
        int? hostPort = 8180)
    {
        var nominatimResource = new NominatimResource(name, mapUrl, hostIpAddress, hostPort.ToString()!);
        
        builder.Services.TryAddLifecycleHook<NominatimResourceLifecycleHook>();
        
        var nominatimResourceBuilder = builder.AddResource(nominatimResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = NominatimImage, Tag = imageTag })
            .PublishAsContainer()
            .WithEnvironment("PBF_URL", mapUrl)
            .WithEnvironment("IMPORT_WIKIPEDIA", importWikipediaData ? "true" : "false")
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

        return nominatimResourceBuilder;
    } 
    public static IResourceBuilder<NominatimResource> WithPersistence(this IResourceBuilder<NominatimResource> builder,
        string nominatimDataVolumeName="nominatim-data",
        string nominatimFlatVolumeName = "nominatim-flat-node",
        string nominatimPostgresqlVolumeName = "nominatim-postgres")
    {
        return builder
            .WithVolume(nominatimDataVolumeName, "/nominatim/data")
            .WithVolume(nominatimFlatVolumeName, "/nominatim/flatnode")
            .WithVolume(nominatimPostgresqlVolumeName, "/var/lib/postgresql/14/main");
    }
}