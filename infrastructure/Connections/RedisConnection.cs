using System.Text.Json;
using domain.Enums;
using domain.Interfaces;
using StackExchange.Redis;

namespace infrastructure.Connections;

public class RedisConnection : IDatabaseConnection
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType => DatabaseType.Redis;

    private ConnectionMultiplexer? _redis;
    private IDatabase? _db;

    public RedisConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }
    
    // ✅ MANTENER tu lógica original de Open
    public async Task Open()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
        _db = _redis.GetDatabase();
    }

    // ✅ MANTENER tu lógica original de Close
    public Task Close()
    {
        _redis?.Dispose();
        return Task.CompletedTask;
    }

    // ✅ MANTENER tu lógica original de ExecuteQuery
    public async Task<string> ExecuteQuery(string query)
    {
        if (_db == null)
            throw new InvalidOperationException("Redis connection not opened.");

        // Interpretar el query como JSON
        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(query)
                   ?? throw new Exception("Query must be JSON.");

        string command = json["command"].ToString()!.ToLower();

        return command switch
        {
            "get" => await ExecuteGet(json),
            "set" => await ExecuteSet(json),
            "keys" => await ExecuteKeys(json),
            _ => throw new NotSupportedException($"Redis command '{command}' not supported.")
        };
    }

    private async Task<string> ExecuteGet(Dictionary<string, object> json)
    {
        string key = json["key"].ToString()!;
        var result = await _db!.StringGetAsync(key);
        
        return result.HasValue
            ? JsonSerializer.Serialize(result.ToString())
            : "null";
    }

    private async Task<string> ExecuteSet(Dictionary<string, object> json)
    {
        string key = json["key"].ToString()!;
        string value = json["value"].ToString()!;

        await _db!.StringSetAsync(key, value);

        return JsonSerializer.Serialize(new { result = "OK" });
    }

    private async Task<string> ExecuteKeys(Dictionary<string, object> json)
    {
        string pattern = json.ContainsKey("pattern") ? json["pattern"].ToString()! : "*";

        var endpoints = _redis!.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server
            .Keys(pattern: pattern)
            .Select(k => k.ToString())
            .ToList();

        return JsonSerializer.Serialize(keys);
    }

    // ✅ NUEVO método TestConnection
    public async Task<bool> TestConnection()
    {
        try
        {
            await Open();
            await _db!.PingAsync();
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
        if (_redis == null)
            throw new InvalidOperationException("Connection not opened.");

        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());
        var keys = server.Keys().Take(100).Select(k => k.ToString()).ToList(); // Limitar a 100 claves

        return new { Keys = keys };
    }
}