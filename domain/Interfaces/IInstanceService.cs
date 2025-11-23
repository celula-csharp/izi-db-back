using domain.Models;

namespace domain.Interfaces;

public interface IInstanceService
{
    Task<InstanceInfo?> GetInstanceAsync(int instanceId);
}
