using System.Security.Claims;
using Application.Instances.Dtos;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly SystemDbContext _context;

    public StudentController(SystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("my-instance")]
    public async Task<IActionResult> GetMyInstance()
    {
        // 1. Obtener el userId desde el token JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? User.FindFirst("sub");

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            // Token sin userId válido → problema de configuración
            return Unauthorized(new
            {
                error = "InvalidToken",
                message = "No se pudo obtener el identificador de usuario del token."
            });
        }

        // 2. Buscar la instancia asignada al usuario
        var userInstance = await _context.UserInstances
            .Include(ui => ui.DatabaseInstance)
            .FirstOrDefaultAsync(ui => ui.UserId == userId);

        if (userInstance == null)
        {
            // No tiene instancia asignada
            return NotFound(new
            {
                error = "NoInstanceAssigned",
                message = "El estudiante aún no tiene una instancia asignada."
            });
        }

        // 3. Mapear a DTO de respuesta
        var dto = new MyInstanceResponseDto
        {
            UserId = userId,
            DatabaseInstanceId = userInstance.DatabaseInstanceId,
            DatabaseName = userInstance.DatabaseInstance?.Name ?? string.Empty,
            DatabaseDescription = userInstance.DatabaseInstance?.Description,
            IsActive = userInstance.DatabaseInstance?.IsActive ?? false,
            AssignedAt = userInstance.AssignedAt
        };

        return Ok(dto);
    }
}
