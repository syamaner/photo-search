using System;
using Aspire.Hosting.ApplicationModel;
using HealthChecks.RabbitMQ;

namespace PhotoSearch.AppHost.WaitFor;
/// <summary>
///
/// Reference: https://github.dev/davidfowl/WaitForDependenciesAspire
/// </summary>
public static class RabbitMqHealthCheckExtensions
{
    /// <summary>
    /// Adds a health check to the RabbitMQ server resource.
    /// </summary>
    public static IResourceBuilder<RabbitMQServerResource> WithHealthCheck(
        this IResourceBuilder<RabbitMQServerResource> builder, string? host)
    {
        return builder.WithAnnotation(HealthCheckAnnotation.Create(cs => !string.IsNullOrWhiteSpace(host)
            ? new RabbitMQHealthCheck(new RabbitMQHealthCheckOptions
                { ConnectionUri = new Uri(cs.Replace("localhost", host)) })
            : new RabbitMQHealthCheck(new RabbitMQHealthCheckOptions { ConnectionUri = new Uri(cs) })));
    }
}