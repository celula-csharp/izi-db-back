using System.Security.Claims;
using api.DTOs;
using domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IPermissionService = application.Interfaces.IPermissionService;
using IQueryExecutor = application.Interfaces.IQueryExecutor;

namespace api.Controllers;

[ApiController]
[Route("api/query")]
public class QueryController : ControllerBase
{
    private readonly IInstanceService _instanceService;
    private readonly IPermissionService _permissionService;
    private readonly IQueryExecutor _queryExecutor;
    
    public QueryController(
        IInstanceService instanceService,
        IPermissionService permissionService,
        IQueryExecutor queryExecutor
    )
    {
        _instanceService = instanceService;
        _permissionService = permissionService;
        _queryExecutor = queryExecutor;
    }

    // POST /api/query/execute
    [HttpPost("execute")]
    [Authorize] 
    public async Task<IActionResult> ExecuteQuery([FromBody] QueryExecuteRequest request)
    {
        // 1. Validar Request
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        if (request.InstanceId <= 0)
            return BadRequest(new { error = "InstanceId must be greater than 0." });

        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest(new { error = "Query cannot be empty." });

        // 2. Obtener datos del usuario
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (userId == null || role == null)
            return StatusCode(401, new { error = "Unauthorized: invalid token." });

        // 3. Obtener instancia
        var instance = await _instanceService.GetInstanceAsync(request.InstanceId);

        if (instance == null)
            return NotFound(new { error = "Instance not found." });

        // 4. Validar permisos
        bool allowed = await _permissionService.CanAccessInstanceAsync(
            userId,
            role,
            instance.OwnerUserId
        );
        
        if (!allowed)
            return Forbid("You do not have permission to access this instance.");

        try
        {
            // 5. Ejecutar query
            var result = await _queryExecutor.ExecuteQueryAsync(
                instance.Engine,
                request.Query,
                instance.ConnectionString,
                userId
            );

            // 6. Respuesta exitosa
            return Ok(new
            {
                instance = request.InstanceId,
                executedBy = userId,
                result
            });
        }
        catch (Exception ex)
        {
            // 7. Error del motor de base de datos
            return BadRequest(new { error = ex.Message });
        }
    }
}