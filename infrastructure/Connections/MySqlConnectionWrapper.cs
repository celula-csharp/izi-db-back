using System.Text.Json;
using domain.Interfaces;
using MySql.Data.MySqlClient;

namespace infrastructure;

public class MySqlConnectionWrapper : IDatabaseConnection
{
    private readonly string _connectionString;
    private MySqlConnection? _connection; 

    public MySqlConnectionWrapper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task Open()
    {
        if (_connection != null) return;
        
        _connection = new MySqlConnection(_connectionString); 
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

    public async Task<List<Dictionary<string, object>>> ExecuteQuery(string query)
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

        return table;
    }
}