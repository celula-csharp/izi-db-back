namespace application.Interfaces;

public interface IPermissionService
{
    Task<bool> CanAccessInstanceAsync(string userId, string role, string ownerUserId);
}