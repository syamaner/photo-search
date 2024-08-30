using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.MapTileServer;

public class MapTileServerResource(string name, int publicPort = 8080, string? entrypoint = null):
    ContainerResource(name, entrypoint), IResourceWithConnectionString
{
    private const string Host = "localhost";
    private const string NominatimEndpointName = "http";
    private EndpointReference? _endpointReference;
    
    public EndpointReference Endpoint =>
        _endpointReference ??= new EndpointReference(this, NominatimEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"http://{Host}:{publicPort.ToString()}");
}