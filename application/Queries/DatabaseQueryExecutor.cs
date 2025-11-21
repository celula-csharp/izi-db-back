using application.Dtos;
using domain.Enums;
using domain.Interfaces;    
using infrastructure.Factory;

namespace application.Queries;

public class DatabaseQueryExecutor : IDatabaseQueryExecutor
{
    private readonly UniversalSqlExecutor _universalExecutor; // ejecutor temporal

    public DatabaseQueryExecutor(
        UniversalSqlExecutor universalExecutor
    )
    {
        _universalExecutor = universalExecutor;
    }

    public async Task<QueryResultDto> ExecuteQueryAsync(DatabaseType engine, string query, string instanceId)
    {
        try
        {
            // Usa el factory actual (que devuelve IDatabaseConnection)
            IDatabaseConnection connection = DatabaseFactory.Create(engine);

            // Mientras las conexiones reales NO existen, solo el universal puede ejecutarlas
            return await _universalExecutor.ExecuteAsync(connection, query);
        }
        catch (Exception ex)
        {
            return new QueryResultDto
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}