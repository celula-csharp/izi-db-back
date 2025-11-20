using System.Text.Json;
using domain.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace infrastructure.Connections;

public class MongoDbConnection : IDatabaseConnection
{
    private readonly string _connectionString;
    private IMongoClient? _client;
    private IMongoDatabase? _database;
    
    public MongoDbConnection(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public Task Open()
    {
        _client = new MongoClient(_connectionString);
        
        // seleccionar la base desde la connection string
        var mongoUrl = new MongoUrl(_connectionString);
        
        string dbName = string.IsNullOrEmpty(mongoUrl.DatabaseName) ? "test" : mongoUrl.DatabaseName;
        
        _database = _client.GetDatabase(dbName);

        return Task.CompletedTask;
    }

    public Task Close()
    {
        //Mongo no usa Close. No se cierra manualmente.
        return Task.CompletedTask;
    }

    public async Task<string> ExecuteQuery(string query)
    {
        if (_database == null)
            throw new InvalidOperationException("Connection not opened.");
        
        //parsear JSON a BSON
        var json = BsonDocument.Parse(query);

        //leer la coleccion
        string collectionName = json["collection"].AsString;
        
        // leer filtro si existe
        var filter = json.Contains("filter") ? json["filter"].AsBsonDocument : new BsonDocument();

        var collection = _database.GetCollection<BsonDocument>(collectionName);

        var results = await collection.Find(filter).ToListAsync();
        
        //convertir cada documento BSON a diccionario plano
        var mapped = results.Select(doc => doc.ToDictionary()).ToList();

        // serializar a JSON como el resto de motores
        return JsonSerializer.Serialize(mapped);
    }
}