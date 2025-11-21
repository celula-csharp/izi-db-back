using application.Dtos;
using domain.Interfaces;

namespace application.Queries;

public class UniversalSqlExecutor
{
    public async Task<QueryResultDto> ExecuteAsync(IDatabaseConnection connection, string query)
    {
        try
        {
            await connection.Open();

            var rawResult = await connection.ExecuteQuery(query);

            var record = new Dictionary<string, object>
            {
                { "result", rawResult }
            };

            return new QueryResultDto
            {
                Success = true,
                Records = new List<Dictionary<string, object>> { record }
            };
        }
        catch (Exception ex)
        {
            return new QueryResultDto { Success = false, Error = ex.Message };
        }
        finally
        {
            await connection.Close();
        }
    }
}