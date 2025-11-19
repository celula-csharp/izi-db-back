using application.Docker.Interfaces;
using Docker.DotNet;
using infrastructure.Docker.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace application.Docker.Services;

public class DockerService : IDockerService, IDisposable
{
    private readonly DockerClient _dockerClient;
    private readonly DockerConfiguration _config;
    private readonly ILogger<DockerService> _logger;

    public DockerService(IOptions<DockerConfiguration> config, ILogger<DockerService> logger)
    {
        _config = config.Value;
        _logger = logger;

        var dockerConfig = new DockerClientConfiguration(new Uri(_config.Host));
        _dockerClient = dockerConfig.CreateClient();

        _logger.LogInformation("DockerService initialized for host: {Host}", _config.Host);
    }
    
    public async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            var version = await _dockerClient.System.GetVersionAsync();
            _logger.LogInformation("Docker version: {Version}", version);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker version check failed");
            return false;
        }
    }

    public async Task<string> GetDockerVersionAsync()
    {
        try
        {
            var version = await _dockerClient.System.GetVersionAsync();
            return version.Version;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker version check failed");
            throw new ApplicationException("Docker version check failed", ex);
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        return await IsDockerRunningAsync();
    }

    public void Dispose()
    {
        _dockerClient?.Dispose();
    }
}