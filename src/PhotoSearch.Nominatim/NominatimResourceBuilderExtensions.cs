using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection;

namespace PhotoSearch.Nominatim;

public static class NominatimResourceBuilderExtensions
{
    private const string NominatimImage = "mediagis/nominatim";

    public static IResourceBuilder<NominatimResource> AddNominatim(this IDistributedApplicationBuilder builder,
        string mapUrl,
        bool isRemoteDockerHost,
        string imageTag = "4.4",
        string name = "Nominatim",
        bool importUkPostcodes = false,
        bool importWikipediaData = false,
        int? hostPort = 8180,
        int containerPort = 8080)
    {
        var nominatimResource = new NominatimResource(name, mapUrl, hostPort.ToString()!);

        builder.Services.AddHealthChecks()
            .AddTypeActivatedCheck<NominatimHealthCheck>("nominatim-healthcheck",
                nominatimResource.ConnectionStringExpression.ValueExpression);

        var nominatimResourceBuilder = builder.AddResource(nominatimResource)
            .WithAnnotation(new ContainerImageAnnotation { Image = NominatimImage, Tag = imageTag })
            .PublishAsContainer()
            .WithContainerName(name)
            .WithHealthCheck("nominatim-healthcheck")
            .WithEnvironment("PBF_URL", mapUrl)
            .WithEnvironment("IMPORT_WIKIPEDIA", importWikipediaData ? "true" : "false")
            .WithEnvironment("IMPORT_GB_POSTCODES", importUkPostcodes ? "true" : "false")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithHttpEndpoint(hostPort, containerPort, "http");

        if (!isRemoteDockerHost) return nominatimResourceBuilder;

        foreach (var resourceAnnotation in nominatimResourceBuilder.Resource.Annotations.Where(x =>
                     x is EndpointAnnotation))
        {
            var endpointAnnotation = (EndpointAnnotation)resourceAnnotation;
            endpointAnnotation.IsProxied = false;
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
          //  .WithVolume(nominatimFlatVolumeName, "/nominatim/flatnode")
            .WithVolume(nominatimPostgresqlVolumeName, "/var/lib/postgresql/14/main");
    }
}