using System.Text.Json;
using domain.Enums;
using domain.Interfaces;
using MySqlConnector; // ✅ Cambiar de MySql.Data.MySqlClient a MySqlConnector

namespace infrastructure.Connections;

public class MySqlConnectionWrapper : IDatabaseConnection
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType => DatabaseType.MySql;

    private MySqlConnection? _connection;

    public MySqlConnectionWrapper(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public async Task Open()
    {
        if (_connection != null) return;
        
        _connection = new MySqlConnection(ConnectionString); 
        await _connection.OpenAsync();                        
    }

    public async Task Close()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async Task<string> ExecuteQuery(string query)
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not opened.");

        try
        {
            using var cmd = new MySqlCommand(query, _connection);
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
        catch (Exception e)
        {
            throw new Exception($"MySQL query failed: {e.Message}");
        }
    }

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

    public async Task<object> GetSchemaAsync()
    {
        if (_connection == null)
            throw new InvalidOperationException("Connection not opened.");

        var tables = new List<object>();
        
        // Obtener tablas
        using var cmd = new MySqlCommand(
            "SELECT TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()", 
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