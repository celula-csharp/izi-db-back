using application.Queries.Dtos;
using domain.Enums;

namespace application.Queries.Interfaces;

public interface IDatabaseQueryExecutor
{
    Task<QueryResultDto> ExecuteQueryAsync(DatabaseType engine, string query, string connectionString);
    Task<object> GetSchemaAsync(DatabaseType engine, string connectionString);
}