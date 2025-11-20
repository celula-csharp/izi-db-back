using domain.Interfaces;
using infrastructure;
using infrastructure.Connections;
using infrastructure.Factory;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IDatabaseFactory _factory;
    
    public TestController(IConfiguration config)
    {
        _config = config;
        _factory = new DatabaseFactory();
        
    }
    
    //sql server
    [HttpGet("sql")]
    public async Task<IActionResult> TestSqlSerer()
    {
        try
        {
            string connString = _config.GetConnectionString("SqlServer")!;
            var conn = _factory.Create("sqlserver", connString);
            
            await conn.Open();
            var result = await conn.ExecuteQuery("SELECT 1 AS TestValue");
            await conn.Close();

            return Ok(new { engine = "SqlServer", result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { engine = "SqlServer", error = ex.Message });
        }
    }
    
    //mysql
    [HttpGet("mysql")]
    public async Task<IActionResult> TestMySql()
    {
        try
        {
            string connString = _config.GetConnectionString("MySql")!;
            var conn = _factory.Create("mysql", connString);
            
            await conn.Open();
            var result = await conn.ExecuteQuery("SELECT 1 AS TestValue");
            await conn.Close();

            return Ok(new { engine = "MySql", result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { engine = "MySql", error = ex.Message });
        }
    }
    
    //postgresql
    [HttpGet("postgres")]
    public async Task<IActionResult> TestPostgres()
    {
        try
        {
            string connString = _config.GetConnectionString("Postgres")!;
            var conn = _factory.Create("postgres", connString);
            
            await conn.Open();
            var result = await conn.ExecuteQuery("SELECT 1 AS TestValue");
            await conn.Close();

            return Ok(new { engine = "Postgres", result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { engine = "Postgres", error = ex.Message });
        }
    }
    
    //mongodb
    [HttpGet("mongo")]
    public async Task<IActionResult> TestMongo()
    {
        try
        {
            string connString = _config.GetConnectionString("Mongo")!;
            var conn = _factory.Create("mongodb", connString);
            
            await conn.Open();

            var query = "{ \"collection\": \"users\", \"filter\": {} }";

            var result = await conn.ExecuteQuery(query);

            await conn.Close();

            return Ok(new { engine = "MongoDB", result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { engine = "MongoDB", error = ex.Message });
        }
    }
    
    //redis
    [HttpGet("redis")]
    public async Task<IActionResult> TestRedis()
    {
        try
        {
            string connString = _config.GetConnectionString("Redis")!;
            var conn = _factory.Create("redis", connString);
            
            await conn.Open();

            var result = await conn.ExecuteQuery("{ \"command\": \"keys\", \"pattern\": \"*\" }");

            await conn.Close();

            return Ok(new { engine = "Redis", result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { engine = "Redis", error = ex.Message });
        }
    }
}