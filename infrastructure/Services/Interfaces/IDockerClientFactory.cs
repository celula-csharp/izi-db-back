using Docker.DotNet;

namespace infrastructure.Services.Interfaces;

public interface IDockerClientFactory
{
    DockerClient CreateClient();
}