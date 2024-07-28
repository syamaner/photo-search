using System;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.Nominatim;

namespace PhotoSearch.AppHost.WaitFor;
/// <summary>
///
/// Reference: https://github.dev/davidfowl/WaitForDependenciesAspire
/// </summary>
public static class NominatimHealthCheckExtensions
{
    public static IResourceBuilder<NominatimResource> WithHealthCheck(
        this IResourceBuilder<NominatimResource> builder)
    {
        return builder.WithAnnotation(HealthCheckAnnotation.Create(cs =>
        {
            Console.WriteLine(cs);
            return new NominatimHealthCheck(cs);
        }));
    }
}