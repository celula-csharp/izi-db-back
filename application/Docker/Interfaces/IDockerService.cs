namespace application.Docker.Interfaces;

public interface IDockerService
{
    Task<bool> IsDockerRunningAsync();
    Task<string> GetDockerVersionAsync();
    Task<bool> TestConnectionAsync();
}