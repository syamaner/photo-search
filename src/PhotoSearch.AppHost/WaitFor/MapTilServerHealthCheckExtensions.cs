using System;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.MapTileServer;

namespace PhotoSearch.AppHost.WaitFor;

public static class MapTilServerHealthCheckExtensions
{
    public static IResourceBuilder<MapTileServerResource> WithHealthCheck(
        this IResourceBuilder<MapTileServerResource> builder)
    {
        return builder.WithAnnotation(HealthCheckAnnotation.Create(cs =>
        {
            Console.WriteLine(cs);
            return new MapTileServerHealthCheck(cs);
        }));
    }
}