namespace domain.Interfaces;

public interface IPermissionService
{
        Task<bool> CanAccessInstanceAsync(string userId, string role, string ownerUserId);
}