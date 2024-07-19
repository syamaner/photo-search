using System;
using System.Text.RegularExpressions;

namespace PhotoSearch.AppHost;

public partial class StartupHelper
{
    public static bool NvidiaDockerEnabled()
    {
        var nvidiaDockerValue = Environment.GetEnvironmentVariable("ENABLE_NVIDIA_DOCKER");
        return !string.IsNullOrEmpty(nvidiaDockerValue);
    }
    public static string GetDockerHostValue(){
        var dockerHostValue = Environment.GetEnvironmentVariable("DOCKER_HOST");
        var dockerHost = string.Empty;

        if (string.IsNullOrEmpty(dockerHostValue)) return dockerHost;

        var match = DockerHostRegex().Match(dockerHostValue);
        if (match.Success)
        {
            dockerHost = match.Value;
        }

        return dockerHost;
    }

    [GeneratedRegex(@"(\d{1,3}\.){3}\d{1,3}")]
    private static partial Regex DockerHostRegex();
}