using application.DTOs;

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
    }
}