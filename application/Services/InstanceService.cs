using domain.Interfaces;
using domain.Models;

namespace application.Services;

public class InstanceService : IInstanceService
{
    private readonly List<InstanceInfo> _instances = new()
    {
        new InstanceInfo { 
            Id = 1, 
            Engine = "mysql",
            ConnectionString = "...",
            OwnerUserId = "student-123"
        },
        new InstanceInfo { 
            Id = 2, 
            Engine = "postgres",
            ConnectionString = "...",
            OwnerUserId = "admin-001"
        }
    };

    public Task<InstanceInfo?> GetInstanceAsync(int instanceId)
    {
        return Task.FromResult(_instances.FirstOrDefault(i => i.Id == instanceId));
    }
}