using Application.Instances.Dtos;
using Application.Instances.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IInstanceAssignmentService _instanceAssignmentService;

    public AdminController(IInstanceAssignmentService instanceAssignmentService)
    {
        _instanceAssignmentService = instanceAssignmentService;
    }

    [HttpPost("assign-instance")]
    public async Task<IActionResult> AssignInstance([FromBody] AssignInstanceRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var success = await _instanceAssignmentService
                .AssignInstanceToUserAsync(request.UserId, request.DatabaseInstanceId);

            if (!success)
            {
                // Ya tenía instancia
                return Conflict(new
                {
                    error = "UserAlreadyHasInstance",
                    message = "El estudiante ya tiene una instancia asignada."
                });
            }

            // Asignación OK
            return Ok(new
            {
                message = "Instancia asignada correctamente.",
                userId = request.UserId,
                databaseInstanceId = request.DatabaseInstanceId
            });
        }
        catch (InvalidOperationException ex)
        {
            // Errores de negocio: usuario no encontrado, no es Student, instancia no existe, etc.
            return BadRequest(new
            {
                error = "BusinessRuleError",
                message = ex.Message
            });
        }
        catch (Exception)
        {
            // Error inesperado
            return StatusCode(500, new
            {
                error = "ServerError",
                message = "Ocurrió un error inesperado al asignar la instancia."
            });
        }
    }
}
