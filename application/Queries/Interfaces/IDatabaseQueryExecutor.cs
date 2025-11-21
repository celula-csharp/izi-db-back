using application.Dtos;
using domain.Enums;

namespace application.Queries;

public interface IDatabaseQueryExecutor
{
    Task<QueryResultDto> ExecuteQueryAsync(DatabaseType engine, string query, string connectionString);
}