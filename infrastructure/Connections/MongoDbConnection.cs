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
    
    // ✅ MANTENER tu lógica original de Open
    public Task Open()
    {
        _client = new MongoClient(ConnectionString);
        var mongoUrl = new MongoUrl(ConnectionString);
        string dbName = string.IsNullOrEmpty(mongoUrl.DatabaseName) ? "test" : mongoUrl.DatabaseName;
        _database = _client.GetDatabase(dbName);
        return Task.CompletedTask;
    }

    // ✅ MANTENER tu lógica original de Close
    public Task Close()
    {
        // MongoDB no requiere cierre manual
        return Task.CompletedTask;
    }

    // ✅ MANTENER tu lógica original de ExecuteQuery
    public async Task<string> ExecuteQuery(string query)
    {
        if (_database == null)
            throw new InvalidOperationException("Connection not opened.");
        
        var json = BsonDocument.Parse(query);
        string collectionName = json["collection"].AsString;
        var filter = json.Contains("filter") ? json["filter"].AsBsonDocument : new BsonDocument();
        var collection = _database.GetCollection<BsonDocument>(collectionName);
        var results = await collection.Find(filter).ToListAsync();
        var mapped = results.Select(doc => doc.ToDictionary()).ToList();
        return JsonSerializer.Serialize(mapped);
    }

    // ✅ NUEVO método TestConnection
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

    // ✅ NUEVO método GetSchemaAsync
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