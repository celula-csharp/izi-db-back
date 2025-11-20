using Docker.DotNet;
using infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace infrastructure.Services.Docker;

public class DockerClientFactory : IDockerClientFactory
{
    private readonly IConfiguration _configuration;

    public DockerClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public DockerClient CreateClient()
    {
        // For Windows
        var dockerUri = new Uri("npipe://./pipe/docker_engine");
        
        // For Linux
        // var dockerUri = new Uri("unix:///var/run/docker.sock");

        return new DockerClientConfiguration(dockerUri)
            .CreateClient();
    }
}