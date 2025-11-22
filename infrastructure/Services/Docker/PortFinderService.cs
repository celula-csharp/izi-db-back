using System.Net;
using System.Net.Sockets;
using domain.Models.Docker;
using Microsoft.Extensions.Logging;

namespace infrastructure.Services.Docker;

public class PortFinderService
{
    private readonly int _startPort = 14330;
    private readonly int _endPort = 14400;
    private readonly ILogger<PortFinderService> _logger;
    private readonly List<PortAllocation> _allocatedPorts = new();

    public PortFinderService(ILogger<PortFinderService> logger)
    {
        _logger = logger;
    }

    public int FindAvailablePort()
    {
        _logger.LogInformation("Buscando puerto disponible en rango {StartPort}-{EndPort}", _startPort, _endPort);
            
        for (int port = _startPort; port <= _endPort; port++)
        {
            if (IsPortAvailable(port) && !IsPortAllocated(port))
            {
                _allocatedPorts.Add(new PortAllocation 
                { 
                    Port = port, 
                    IsAvailable = false,
                    AllocatedAt = DateTime.UtcNow
                });
                    
                _logger.LogInformation("Puerto {Port} asignado exitosamente", port);
                return port;
            }
        }
            
        throw new InvalidOperationException($"No hay puertos disponibles en el rango {_startPort}-{_endPort}");
    }

    public void ReleasePort(int port)
    {
        var allocation = _allocatedPorts.FirstOrDefault(p => p.Port == port);
        if (allocation != null)
        {
            _allocatedPorts.Remove(allocation);
            _logger.LogInformation("Puerto {Port} liberado", port);
        }
    }

    private bool IsPortAvailable(int port)
    {
        try
        {
            using var tcpListener = new TcpListener(IPAddress.Loopback, port);
            tcpListener.Start();
            tcpListener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private bool IsPortAllocated(int port)
    {
        return _allocatedPorts.Any(p => p.Port == port && !p.IsAvailable);
    }

    public List<PortAllocation> GetAllocatedPorts()
    {
        return _allocatedPorts.ToList();
    }
}