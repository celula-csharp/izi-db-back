using Microsoft.Extensions.Configuration;

namespace infrastructure.Docker.Configuration;

public class DockerConfiguration
{
    public const string SectionName = "Docker";
    
    public string Host { get; set; } = "npipe://./pipe/docker_engine";
    public int PortRangeStart { get; set; } = 14330;
    public int PortRangeEnd { get; set; } = 14400;
    public int CommandTimeoutSeconds { get; set; } = 30;
}

public static class DockerConfigurationExtensions
{
    public static DockerConfiguration GetDockerConfiguration(this IConfiguration configuration)
    {
        var dockerConfig = new DockerConfiguration();
        configuration.GetSection(DockerConfiguration.SectionName).Bind(dockerConfig);
        return dockerConfig;
    }
}