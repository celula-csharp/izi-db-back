using application.Docker.Interfaces;
using infrastructure.Docker.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace infrastructure.Docker.Services;

public class PortFinderService : IPortFinderService
{
    private readonly DockerConfiguration _config;
    private readonly ILogger<PortFinderService> _logger;
    private readonly HashSet<int> _usedPorts = new();

    public PortFinderService(IOptions<DockerConfiguration> config, ILogger<PortFinderService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }
    
    public int FindAvailablePort()
    {
        throw new NotImplementedException();
    }

    public bool IsPortAvailable(int port)
    {
        throw new NotImplementedException();
    }

    public List<int> GetAvailablePorts(int count)
    {
        throw new NotImplementedException();
    }
}