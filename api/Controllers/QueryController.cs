using application.Queries.Interfaces;
using domain.Enums;
using api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/query")]
public class QueryController : ControllerBase
{
    private readonly IDatabaseQueryExecutor _queryExecutor;

    public QueryController(IDatabaseQueryExecutor queryExecutor)
    {
        _queryExecutor = queryExecutor;
    }

    //Ejecutar queries
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteQuery([FromBody] ExecuteQueryRequest request)
    {
        try
        {
            if (!Enum.TryParse<DatabaseType>(request.Engine, true, out var engine))
            {
                return BadRequest(new { error = $"Motor no soportado: {request.Engine}" });
            }

            var result = await _queryExecutor.ExecuteQueryAsync(engine, request.Query, request.ConnectionString);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    //Schema Discovery
    [HttpGet("schema")]
    public async Task<IActionResult> GetSchema([FromQuery] string engine, [FromQuery] string connectionString)
    {
        try
        {
            if (!Enum.TryParse<DatabaseType>(engine, true, out var databaseType))
            {
                return BadRequest(new { error = $"Motor no soportado: {engine}" });
            }

            var schema = await _queryExecutor.GetSchemaAsync(databaseType, connectionString);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}