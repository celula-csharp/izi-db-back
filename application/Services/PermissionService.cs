using application.Interfaces;

namespace application.Services;

public class PermissionService : IPermissionService
{
    public async Task<bool> CanAccessInstanceAsync(string userId, string role, string ownerUserId)
    {
        await Task.CompletedTask;

        if (role == "admin")
            return true;

        return userId == ownerUserId;
    }
}