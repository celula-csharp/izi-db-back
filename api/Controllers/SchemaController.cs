using Microsoft.AspNetCore.Mvc;
using infrastructure.Factory;
using domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [ApiController]
    [Route("api/schema")]
    public class SchemaController : ControllerBase
    {
        private readonly IDatabaseFactory _factory;

        public SchemaController(IDatabaseFactory factory)
        {
            _factory = factory;
        }
        
        //  GET /api/schema/{engine}
        [HttpGet("{engine}")]
        [Authorize] 
        public async Task<IActionResult> GetSchema(string engine)
        {
            if (string.IsNullOrWhiteSpace(engine))
                return BadRequest(new { error = "Engine is required." });

            string? connString = GetConnectionString(engine);

            if (connString == null)
                return BadRequest(new { error = $"Engine '{engine}' not supported." });

            var conn = _factory.Create(engine, connString);

            if (conn == null)
                return BadRequest(new { error = $"Engine '{engine}' not implemented." });

            try
            {
                await conn.Open();

                List<Dictionary<string, object>> result;

                switch (engine.ToLower())
                {
                    case "sqlserver":
                        result = await conn.ExecuteQuery("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
                        break;

                    case "mysql":
                        result = await conn.ExecuteQuery("SHOW TABLES");
                        break;

                    case "postgres":
                        result = await conn.ExecuteQuery("SELECT table_name FROM information_schema.tables WHERE table_schema='public'");
                        break;

                    case "mongodb":
                        result = await conn.ExecuteQuery("{ \"listCollections\": 1 }");
                        break;

                    case "redis":
                        result = await conn.ExecuteQuery("{ \"command\": \"KEYS\", \"pattern\": \"*\" }");
                        break;

                    default:
                        return BadRequest(new { error = "Engine not recognized." });
                }

                await conn.Close();

                return Ok(new
                {
                    engine,
                    schema = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { engine, error = ex.Message });
            }
        }
        
        // Helper para conexiones
        private string? GetConnectionString(string engine)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return engine.ToLower() switch
            {
                "sqlserver" => config.GetConnectionString("SqlServer"),
                "mysql"     => config.GetConnectionString("MySql"),
                "postgres"  => config.GetConnectionString("Postgres"),
                "mongodb"   => config.GetConnectionString("Mongo"),
                "redis"     => config.GetConnectionString("Redis"),
                _ => null
            };
        }
    }
}
