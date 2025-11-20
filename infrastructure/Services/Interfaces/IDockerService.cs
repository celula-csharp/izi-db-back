using domain.Models.Docker;

namespace infrastructure.Services.Interfaces;

public interface IDockerService
{
    Task<DockerContainer> CreateContainerAsync(string image, string databaseType, string credentials);
    Task StartContainerAsync(string containerId);
    Task StopContainerAsync(string containerId);
    Task DeleteContainerAsync(string containerId);
    Task<string> GetContainerLogsAsync(string containerId);
    Task<DockerContainer> GetContainerStatusAsync(string containerId);
}