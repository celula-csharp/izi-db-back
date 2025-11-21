using application.Dtos;
using System.Data.Common;

namespace application.Queries;

public class SqlQueryExecutor
{
    public async Task<QueryResultDto> ExecuteAsync(DbConnection connection, string query)
    {
        try
        {
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = query;

            using var reader = await command.ExecuteReaderAsync();

            var records = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var data = new Dictionary<string, object>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    data[reader.GetName(i)] = reader.GetValue(i);
                }

                records.Add(data);
            }

            return new QueryResultDto
            {
                Success = true,
                Records = records
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
        finally
        {
            await connection.CloseAsync();
        }
    }
}