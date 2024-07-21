using System;
using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.Ollama;

/// <summary>
/// Reference: https://raygun.com/blog/enhancing-aspire-with-ai-with-ollama/
/// </summary>
public class OllamaResource : ContainerResource, IResourceWithConnectionString
{
    private readonly string _host;
    private readonly string _publicPort;

    public OllamaResource(string name, string modelName, string externalHostIpAddress, string publicPort,
        string? entrypoint = null) : base(name, entrypoint)
    {
        if (string.IsNullOrWhiteSpace(publicPort)) throw new ArgumentNullException(nameof(publicPort));
        if (string.IsNullOrWhiteSpace(modelName)) throw new ArgumentNullException(nameof(modelName));
        
        ModelName = modelName;
        _host =
            string.IsNullOrWhiteSpace(externalHostIpAddress) ? "localhost" : externalHostIpAddress;
        _publicPort = publicPort;
    }

    private const string OllamaEndpointName = "ollama";

    private EndpointReference? _endpointReference;

    public EndpointReference Endpoint =>
        _endpointReference ??= new EndpointReference(this, OllamaEndpointName);

    public string ModelName { get; }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{_host}:{_publicPort}"
        );
}