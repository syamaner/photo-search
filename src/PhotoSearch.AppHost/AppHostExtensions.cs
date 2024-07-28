using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using PhotoSearch.AppHost.WaitFor;

namespace PhotoSearch.AppHost;

public static class AppHostExtensions
{
    public static IResourceBuilder<PostgresServerResource> AddPostgreSql(this IDistributedApplicationBuilder builder,
        string dbName, int publicPort, string? host)
    {
        var pgUsername = builder.AddParameter("pgUsername", secret: true);
        var pgPassword = builder.AddParameter("pgPassword", secret: true);
        var postgresContainer = builder.AddPostgres("postgres", pgUsername, pgPassword, port: publicPort)
            .WithDataVolume("postgres", false);

        var pgAdminContainer = builder.AddContainer("pgadmin", "dpage/pgadmin4")
            .WithEnvironment("PGADMIN_DEFAULT_EMAIL", "a@a.com")
            .WithEnvironment("PGADMIN_DEFAULT_PASSWORD", pgPassword.Resource.Value)
            .WithReference(postgresContainer)
            .WithVolume("pgadmin-data", "/var/lib/pgadmin");

        if (string.IsNullOrWhiteSpace(host)) return postgresContainer;

        var pgConnectionStringRedirection =
            new CustomPostgresConnectionStringRedirection(dbName, host, publicPort.ToString(), pgUsername, pgPassword);

        postgresContainer.WithConnectionStringRedirection(pgConnectionStringRedirection);
        // postgresContainer.WithEndpoint(5432, 5432, "tcp", "db", isExternal: true, isProxied: false);
        postgresContainer.WithContainerRuntimeArgs("-p", $"0.0.0.0:{publicPort}:{publicPort}");
        pgAdminContainer
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:8008:80");

        return postgresContainer;
    }

    public static IResourceBuilder<RabbitMQServerResource> AddRabbitMq(this IDistributedApplicationBuilder builder,
        string name, string? host = null, int publicPort = 5672, params IResourceBuilder<ProjectResource>[]? projects)
    {
        var rmqUsername = builder.AddParameter("rmqUsername", secret: true);
        var rmqPassword = builder.AddParameter("rmqPassword", secret: true);

        var messaging = builder.AddRabbitMQ(name, rmqUsername, rmqPassword, publicPort)
            .WithManagementPlugin();

        if (projects != null)
        {
            foreach (var project in projects)
            {
                project.WithReference(messaging);
                project.WaitFor(messaging);
            }
        }

        if (string.IsNullOrWhiteSpace(host)) return messaging;

        messaging
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:{publicPort}:{publicPort}")
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:15672:15672");

        if (projects == null) return messaging;

        var connectionString =
            $"amqp://{rmqUsername.Resource.Value}:{rmqPassword.Resource.Value}@{host}:{publicPort}";
        var connectionStringKey = $"ConnectionStrings__{name}";
        foreach (var project in projects)
        {
            project
                .WithEnvironment(connectionStringKey, connectionString);
        }

        return messaging;
    }


    public static IResourceBuilder<RabbitMQServerResource> UpdateRabbitmqConnectionString(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<ProjectResource> project, IResourceBuilder<RabbitMQServerResource> messaging, string host)
    {
        var rmqUsername = messaging.Resource.UserNameParameter;
        var rmqPassword = messaging.Resource.PasswordParameter;

        if (string.IsNullOrWhiteSpace(host)) return messaging;

        messaging
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:{messaging.Resource.PrimaryEndpoint.Port}:{messaging.Resource.PrimaryEndpoint.Port}")
            .WithContainerRuntimeArgs("-p", $"0.0.0.0:15672:15672");

        var connectionString =
            $"amqp://{rmqUsername!.Value}:{rmqPassword.Value}@{host}:{messaging.Resource.PrimaryEndpoint.Port}";
        var connectionStringKey = $"ConnectionStrings__{messaging.Resource.Name}";
        project
            .WithEnvironment(connectionStringKey, connectionString);
        return messaging;
    }
}