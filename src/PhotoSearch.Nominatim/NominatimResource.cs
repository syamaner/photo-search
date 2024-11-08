using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.Nominatim;

public class NominatimResource(
    string name,
    string mapsDownloadUrl,
    string port = "8180",
    string? entrypoint = null)
    : ContainerResource(name, entrypoint), IResourceWithConnectionString
{
    private readonly string _host = "localhost";
    private EndpointReference? _endpointReference;
    private const string NominatimEndpointName = "http";

    public string MapsDownloadUrl { get; } = mapsDownloadUrl;
    public EndpointReference Endpoint =>
        _endpointReference ??= new EndpointReference(this, NominatimEndpointName);
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{_host}:{port}"
        );
}