using System.Text.Json;
using domain.Enums;
using domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace infrastructure.Connections;

public class MongoDbConnection : IDatabaseConnection
{
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType => DatabaseType.MongoDb;

    private IMongoClient? _client;
    private IMongoDatabase? _database;
    
    public MongoDbConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public Task Open()
    {
        _client = new MongoClient(ConnectionString);
        var mongoUrl = new MongoUrl(ConnectionString);
        string dbName = string.IsNullOrEmpty(mongoUrl.DatabaseName) ? "test" : mongoUrl.DatabaseName;
        _database = _client.GetDatabase(dbName);
        return Task.CompletedTask;
    }

    public Task Close()
    {
        return Task.CompletedTask;
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQuery(string query)
    {
        if (_database == null) // Cambiado de _db a _database
            throw new Exception("MongoDB not opened.");

        var doc = JsonSerializer.Deserialize<JsonElement>(query);

        string collection = doc.GetProperty("collection").GetString()!;
        var filter = doc.GetProperty("filter").GetRawText();

        var bsonFilter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filter);
        var coll = _database.GetCollection<BsonDocument>(collection); // Cambiado de _db a _database

        var result = await coll.Find(bsonFilter).ToListAsync();

        return result.Select(r => r.ToDictionary()).ToList();
    }

    public async Task<bool> TestConnection()
    {
        try
        {
            await Open();
            await _database!.ListCollectionNamesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<object> GetSchemaAsync()
    {
        if (_database == null)
            throw new InvalidOperationException("Connection not opened.");

        var collections = new List<object>();
        var collectionsCursor = await _database.ListCollectionNamesAsync();
        var collectionNames = await collectionsCursor.ToListAsync();

        foreach (var name in collectionNames)
        {
            collections.Add(new { Name = name });
        }

        return new { Collections = collections };
    }
}