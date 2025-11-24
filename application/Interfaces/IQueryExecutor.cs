using application.DTOs;
using domain.Enums;

namespace application.Interfaces
{
    public interface IQueryExecutor
    {
        Task<QueryResultDto> ExecuteQueryAsync(
            string engine,
            string query,
            string connectionString,
            string userId
        );
        
        Task<object> GetSchemaAsync(DatabaseType engine, string connectionString, string userId);
    }
}
