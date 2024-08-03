using System;
using System.Collections.Generic;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace PhotoSearch.AppHost;

public class SShUtility: IDisposable
{

    private readonly List<ForwardedPortLocal> forwardedPorts = new();

    private readonly SshClient client = null;
    public SShUtility(string host, string username, string sshKeyPath)
    { 
        var pk = new PrivateKeyFile(sshKeyPath);
        IPrivateKeySource[] keyFiles = new[] { pk };

        client = new SshClient(host, username, keyFiles);
    }
    
    public void Connect()
    {
        client.Connect();
    }
    
    public void AddForwardedPort(int localPort, int remotePort)
    {
        var port = new ForwardedPortLocal("localhost", (uint)localPort, "localhost", (uint)remotePort);
        client.AddForwardedPort(port);
        port.Exception += delegate(object sender, ExceptionEventArgs e)
        {
            Console.WriteLine(e.Exception.ToString());
        };
        port.Start();
    }

    public void Dispose()
    {
        foreach (var forwardedPort in forwardedPorts)
        {
            forwardedPort.Stop();
        }
        client?.Disconnect();
        client?.Dispose();
    }
}
 