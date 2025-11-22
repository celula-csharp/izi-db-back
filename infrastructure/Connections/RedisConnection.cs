using System.Text.Json;
using domain.Interfaces;
using StackExchange.Redis;

namespace infrastructure.Connections;

public class RedisConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private ConnectionMultiplexer? _redis;
    private IDatabase? _db;

    public RedisConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task Open()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
        _db = _redis.GetDatabase();
    }

    public Task Close()
    {
        _redis?.Dispose();
        return Task.CompletedTask;
    }
    
    public async Task<List<Dictionary<string, object>>> ExecuteQuery(string query)
    {
        if (_db == null)
            throw new InvalidOperationException("Redis connection not opened.");

        var json = JsonSerializer.Deserialize<Dictionary<string, object>>(query)
                   ?? throw new Exception("Query must be valid JSON.");

        string command = json["command"].ToString()!.ToLower();

        return command switch
        {
            "get"  => await ExecuteGet(json),
            "set"  => await ExecuteSet(json),
            "keys" => await ExecuteKeys(json),
            _ => throw new NotSupportedException($"Redis command '{command}' not supported.")
        };
    }

    private async Task<List<Dictionary<string, object>>> ExecuteGet(Dictionary<string, object> json)
    {
        string key = json["key"].ToString()!;
        var result = await _db!.StringGetAsync(key);

        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "key", key },
                { "value", result.HasValue ? result.ToString()! : null }
            }
        };
    }

    private async Task<List<Dictionary<string, object>>> ExecuteSet(Dictionary<string, object> json)
    {
        string key = json["key"].ToString()!;
        string value = json["value"].ToString()!;

        await _db!.StringSetAsync(key, value);

        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "result", "OK" },
                { "key", key },
                { "value", value }
            }
        };
    }

    private async Task<List<Dictionary<string, object>>> ExecuteKeys(Dictionary<string, object> json)
    {
        string pattern = json.ContainsKey("pattern") ? json["pattern"].ToString()! : "*";

        var endpoints = _redis!.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server
            .Keys(pattern: pattern)
            .Select(k => k.ToString())
            .ToList();

        var records = new List<Dictionary<string, object>>();

        foreach (var key in keys)
        {
            records.Add(new Dictionary<string, object>
            {
                { "key", key }
            });
        }

        return records;
    }
}
