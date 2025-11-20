using Application.Auth.Dtos;
using Application.Auth.Services;
using IziDbBack.Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Auth
{
    public class AuthService : IAuthService
    {
        private readonly SystemDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(SystemDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email);

            if (exists)
                throw new Exception("El usuario o el correo ya existen.");

            var studentRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Student");

            if (studentRole == null)
                throw new Exception("No existe el rol 'Student' en la base de datos.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = dto.Password, // TODO: hash real
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                RoleId = studentRole.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role!.Name
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u =>
                    (u.Email == dto.EmailOrUsername || u.Username == dto.EmailOrUsername) &&
                    u.PasswordHash == dto.Password);

            if (user == null)
                throw new Exception("Credenciales inválidas.");

            if (!user.IsActive)
                throw new Exception("El usuario está inactivo.");

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role!.Name
            };
        }
    }
}
