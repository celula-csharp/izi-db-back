using IziDbBack.Domain.Entities;

namespace Application.Auth.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}