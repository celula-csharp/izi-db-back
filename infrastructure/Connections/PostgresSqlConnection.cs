using System.Text.Json;
using domain.Enums;
using domain.Interfaces;
using Npgsql;

namespace infrastructure.Connections;

public class PostgresSqlConnection : IDatabaseConnection
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType => DatabaseType.PostgreSql;

    private NpgsqlConnection? _connection;
    
    public PostgresSqlConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }
    
    // ✅ MANTENER tu lógica original de Open
    public async Task Open()
    {
        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
    }

    // ✅ MANTENER tu lógica original de Close
    public async Task Close()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }

    // ✅ MANTENER tu lógica original de ExecuteQuery
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

    // ✅ NUEVO método TestConnection
    public async Task<bool> TestConnection()
    {
        try
        {
            await Open();
            await Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // ✅ NUEVO método GetSchemaAsync
    public async Task<object> GetSchemaAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not opened.");

        var tables = new List<object>();
        
        using var cmd = new NpgsqlCommand(
            "SELECT table_name, table_type FROM information_schema.tables WHERE table_schema = 'public'", 
            _connection);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            tables.Add(new
            {
                Name = reader.GetString(0),
                Type = reader.GetString(1)
            });
        }

        return new { Tables = tables };
    }
}