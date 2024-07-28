using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.AppHost;

public class CustomPostgresConnectionStringRedirection:IResourceWithConnectionString
{
    public string Name { get; }
    public ResourceAnnotationCollection Annotations { get; }
    public ReferenceExpression ConnectionStringExpression { get; }
 

    public CustomPostgresConnectionStringRedirection(string name, string externalHostIpAddress, string publicPort,
         IResourceBuilder<ParameterResource> pgUsername,
        IResourceBuilder<ParameterResource> pgPassword, string dbName = "photo-db")
    {
        Name = name;
        Annotations = new ResourceAnnotationCollection();
        var dockerHost = string.IsNullOrWhiteSpace(externalHostIpAddress) ? "localhost" : externalHostIpAddress;
 
        ConnectionStringExpression = ReferenceExpression.Create(
            $"Host={dockerHost};Port={publicPort};Username={pgUsername.Resource.Value};Password={pgPassword.Resource.Value};Database={dbName}"
        );

    }
}
