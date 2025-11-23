using application.Queries.Dtos;
using application.Queries.Interfaces;
using domain.Enums;
using domain.Interfaces;
using infrastructure.Factory;

namespace application.Queries;

public class DatabaseQueryExecutor : IDatabaseQueryExecutor
{
    private readonly IDatabaseFactory _databaseFactory;

    public DatabaseQueryExecutor(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public async Task<QueryResultDto> ExecuteQueryAsync(DatabaseType engine, string query, string connectionString)
    {
        try
        {
            // ✅ Usar el factory inyectado
            var connection = _databaseFactory.Create(engine.ToString().ToLower(), connectionString);
            
            await connection.Open();
            var result = await connection.ExecuteQuery(query);
            await connection.Close();

            var records = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(result);
            
            return new QueryResultDto
            {
                Success = true,
                Records = records ?? new List<Dictionary<string, object>>()
            };
        }
        catch (Exception ex)
        {
            return new QueryResultDto
            {
                Success = false,
                Error = $"Error executing query: {ex.Message}"
            };
        }
    }

    public async Task<object> GetSchemaAsync(DatabaseType engine, string connectionString)
    {
        try
        {
            var connection = _databaseFactory.Create(engine.ToString().ToLower(), connectionString);
            await connection.Open();
            var schema = await connection.GetSchemaAsync();
            await connection.Close();
            
            return schema;
        }
        catch (Exception ex)
        {
            return new { Error = $"Error getting schema: {ex.Message}" };
        }
    }
}