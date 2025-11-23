using System.Text.Json;
using domain.Enums;
using domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace infrastructure.Connections;

public class SqlServerConnection : IDatabaseConnection
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType => DatabaseType.SqlServer;

    private SqlConnection? _connection;
    
    public SqlServerConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }
    public async Task Open()
    {
        _connection = new SqlConnection(ConnectionString);
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

        if (_connection == null)
            throw new InvalidOperationException("Connection not opened.");

        using var cmd = new SqlCommand(query, _connection);
        using var reader = await cmd.ExecuteReaderAsync();
        
        var table = new List<Dictionary<string, object>>();

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
        
        using var cmd = new SqlCommand(
            "SELECT TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES", 
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