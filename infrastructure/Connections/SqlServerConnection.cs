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

    public async Task<string> ExecuteQuery(string query)
    {
        using var cmd = new SqlCommand(query, _connection);
        using var reader = await cmd.ExecuteReaderAsync();
        
        var table = new List<Dictionary<string, object>>();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            table.Add(row);
        }
        return JsonSerializer.Serialize(table);
    }
}