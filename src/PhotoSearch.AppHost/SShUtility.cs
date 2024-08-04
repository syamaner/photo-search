using System;
using System.Collections.Generic;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace PhotoSearch.AppHost;

public class SShUtility: IDisposable
{
    private readonly List<ForwardedPortLocal> _forwardedPorts = [];

    private readonly SshClient _client;
    public SShUtility(string host, string username, string sshKeyPath)
    { 
        var pk = new PrivateKeyFile(sshKeyPath);
        IPrivateKeySource[] keyFiles = { pk };

        _client = new SshClient(host, username, keyFiles);
    }
    
    public void Connect()
    {
        _client.Connect();
    }
    
    public void AddForwardedPort(int localPort, int remotePort)
    {
        var port = new ForwardedPortLocal("localhost", (uint)localPort, "localhost", (uint)remotePort);
        _client.AddForwardedPort(port);
        _forwardedPorts.Add(port);
        port.Exception += delegate(object? sender, ExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        };
        port.Start();
    }

    public void Dispose()
    {
        foreach (var forwardedPort in _forwardedPorts)
        {
            forwardedPort.Stop();
        }
        _client?.Disconnect();
        _client?.Dispose();
    }
}
 