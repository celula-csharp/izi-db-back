using System.Threading.Tasks;
using Application.Auth.Dtos;

namespace Application.Auth.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}