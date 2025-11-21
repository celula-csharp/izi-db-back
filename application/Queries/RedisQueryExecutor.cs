using application.Dtos;
using StackExchange.Redis;

namespace application.Queries
{
    public class RedisQueryExecutor
    {
        public async Task<QueryResultDto> ExecuteAsync(IDatabase db, string query)
        {
            try
            {
                var result = await db.ExecuteAsync(query);

                return new QueryResultDto
                {
                    Success = true,
                    Records = new List<Dictionary<string, object>>
                    {
                        new() { { "result", result.ToString()! } }
                    }
                };
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
}