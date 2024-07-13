using System;
using Aspire.Hosting.ApplicationModel;

namespace PhotoSearch.Ollama;

public class OllamaResource : ContainerResource, IResourceWithConnectionString
{
    private readonly string _externalHostIpAddress;
    private readonly string _publicPort;

    public OllamaResource(string name, string modelName, string externalHostIpAddress, string publicPort,
        string? entrypoint = null) : base(name, entrypoint)
    {
        if (string.IsNullOrWhiteSpace(externalHostIpAddress)) throw new ArgumentNullException(nameof(externalHostIpAddress));
        if (string.IsNullOrWhiteSpace(publicPort)) throw new ArgumentNullException(nameof(publicPort));

        if (string.IsNullOrWhiteSpace(modelName))
        {
            ModelName = "llava:7b";
        }

        ModelName = modelName;
        _externalHostIpAddress = externalHostIpAddress;
        _publicPort = publicPort;
    }

    private const string OllamaEndpointName = "ollama";

    private EndpointReference? _endpointReference;

    public EndpointReference Endpoint =>
        _endpointReference ??= new EndpointReference(this, OllamaEndpointName);

    public string ModelName { get; }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{_externalHostIpAddress}:{_publicPort}"
        );
}