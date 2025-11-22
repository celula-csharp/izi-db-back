using domain.Interfaces;
using MongoDB.Driver;
using System.Text.Json;

namespace infrastructure.Connections;

public class MongoDbConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private IMongoDatabase? _db;

    public MongoDbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Task Open()
    {
        var client = new MongoClient(_connectionString);

        string dbName = MongoUrl.Create(_connectionString).DatabaseName 
                        ?? "default";

        _db = client.GetDatabase(dbName);
        return Task.CompletedTask;
    }

    public Task Close()
    {
        return Task.CompletedTask; // Mongo no necesita cerrar
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQuery(string query)
    {
        if (_db == null)
            throw new Exception("MongoDB not opened.");

        // Query viene así:
        // { "collection": "users", "filter": { "age": { "$gt": 20 } } }

        var doc = JsonSerializer.Deserialize<JsonElement>(query);

        string collection = doc.GetProperty("collection").GetString()!;
        var filter = doc.GetProperty("filter").GetRawText();

        var bsonFilter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<MongoDB.Bson.BsonDocument>(filter);
        var coll = _db.GetCollection<MongoDB.Bson.BsonDocument>(collection);

        var result = await coll.Find(bsonFilter).ToListAsync();

        return result.Select(r => r.ToDictionary()).ToList();
    }
}
