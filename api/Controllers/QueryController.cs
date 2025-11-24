using System.Security.Claims;
using application.Interfaces;
using api.DTOs;
using domain.Enums;
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

    [HttpPost("execute")]
    [Authorize]
    public async Task<IActionResult> ExecuteQuery([FromBody] QueryExecuteRequest request)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required." });

        if (request.InstanceId <= 0)
            return BadRequest(new { error = "InstanceId must be greater than 0." });

        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest(new { error = "Query cannot be empty." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (userId == null || role == null)
            return StatusCode(401, new { error = "Unauthorized: invalid token." });

        var instance = await _instanceService.GetInstanceAsync(request.InstanceId);

        if (instance == null)
            return NotFound(new { error = "Instance not found." });

        bool allowed = await _permissionService.CanAccessInstanceAsync(
            userId,
            role,
            instance.OwnerUserId
        );

        if (!allowed)
            return Forbid("You do not have permission to access this instance.");

        try
        {
            // Convertir DatabaseType a string para compatibilidad
            var result = await _queryExecutor.ExecuteQueryAsync(
                instance.Engine.ToString().ToLower(), // Convertir a string
                request.Query,
                instance.ConnectionString,
                userId
            );

            return Ok(new
            {
                instance = request.InstanceId,
                executedBy = userId,
                result
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("schema")]
    [Authorize]
    public async Task<IActionResult> GetSchema([FromQuery] int instanceId)
    {
        if (instanceId <= 0)
            return BadRequest(new { error = "InstanceId must be greater than 0." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (userId == null || role == null)
            return StatusCode(401, new { error = "Unauthorized: invalid token." });

        var instance = await _instanceService.GetInstanceAsync(instanceId);

        if (instance == null)
            return NotFound(new { error = "Instance not found." });

        bool allowed = await _permissionService.CanAccessInstanceAsync(
            userId,
            role,
            instance.OwnerUserId
        );

        if (!allowed)
            return Forbid("You do not have permission to access this instance.");

        try
        {
            // Convertir string a DatabaseType
            if (!Enum.TryParse<DatabaseType>(instance.Engine, true, out var engineType))
            {
                return BadRequest(new { error = $"Invalid engine type: {instance.Engine}" });
            }

            var schema = await _queryExecutor.GetSchemaAsync(
                engineType, // Ahora es DatabaseType
                instance.ConnectionString,
                userId
            );
            return Ok(schema);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}