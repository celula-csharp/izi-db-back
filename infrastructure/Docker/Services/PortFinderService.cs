using System.Net;
using System.Net.Sockets;
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
        for (int port = _config.PortRangeStart; port <= _config.PortRangeEnd; port++)
        {
            if (IsPortAvailable(port) && !_usedPorts.Contains(port))
            {
                _usedPorts.Add(port);
                _logger.LogInformation("Port {Port} is available", port);
                return port;
            }
        }

        throw new InvalidOperationException(
            $"There is not available ports in the range {_config.PortRangeStart}-{_config.PortRangeEnd}");
    }

    public bool IsPortAvailable(int port)
    {
        try
        {
            // Try connect as a client.
            using var client = new TcpClient();
            client.Connect(IPAddress.Loopback, port);
            return false;
        }
        catch (SocketException)
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying port {Port}", port);
            return false;
        }
    }

    public List<int> GetAvailablePorts(int count)
    {
        var availablePorts = new List<int>();

        for (int port = _config.PortRangeStart; port <= _config.PortRangeEnd && availablePorts.Count < count; port++)
        {
            if (IsPortAvailable(port) && !_usedPorts.Contains(port))
            {
                availablePorts.Add(port);
                _usedPorts.Add(port);
            }
        }

        if (availablePorts.Count < count)
        {
            throw new InvalidOperationException(
                $"Not enough ports are available. {count} are required, {availablePorts.Count} are available.");
        }
        
        _logger.LogInformation("{Count} available ports were assigned.", availablePorts.Count);
        return availablePorts;
    }

    public void ReleasePort(int port)
    {
        _usedPorts.Remove(port);
        _logger.LogInformation("{Port} was killed.", port);
    }
}