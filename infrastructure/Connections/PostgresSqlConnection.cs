using System.Text.Json;
using domain.Interfaces;
using Npgsql;

namespace infrastructure.Connections;

public class PostgresSqlConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private NpgsqlConnection? _connection;
    
    public PostgresSqlConnection(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task Open()
    {
        _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
    }

    public async Task Close()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }

    public async Task<string> ExecuteQuery(string query)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not opened.");

        using var cmd = new NpgsqlCommand(query, _connection);
        using var reader = await cmd.ExecuteReaderAsync();

        var rows = new List<Dictionary<string, object>>();

        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }

            rows.Add(row);
        }
        return JsonSerializer.Serialize(rows);
    }
}