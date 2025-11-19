namespace application.Docker.Interfaces;

public interface IPortFinderService
{
    int FindAvailablePort();
    bool IsPortAvailable(int port);
    List<int> GetAvailablePorts(int count);
}