using System.Text.Json;
using domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace infrastructure.Connections;

public class SqlServerConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    public SqlServerConnection(string connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task Open()
    {
        _connection  = new SqlConnection(_connectionString);
        await _connection.OpenAsync(); 
    }

    public async Task Close()
    {
        if (_connection != null)
            await _connection.CloseAsync(); 
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQuery(string query)
    {
        var result = new List<Dictionary<string, object>>();

        using var cmd = new SqlCommand(query, _connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                row[reader.GetName(i)] = value;
            }

            result.Add(row);
        }

        return result;
    }
}