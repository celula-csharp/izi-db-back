using application.Dtos;
using MongoDB.Bson;
using MongoDB.Driver;

namespace application.Queries;

public class MongoQueryExecutor
{
    public async Task<QueryResultDto> ExecuteAsync(IMongoDatabase database, string query)
    {
        try
        {
            var doc = BsonDocument.Parse(query);
            var collectionName = doc["collection"].AsString;
            var filter = doc["filter"].AsBsonDocument;

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var result = await collection.Find(filter).ToListAsync();

            var records = result
                .Select(d => d.ToDictionary())
                .ToList();

            return new QueryResultDto
            {
                Success = true,
                Records = records
            };
        }
        catch (Exception ex)
        {
            return new QueryResultDto { Success = false, Error = ex.Message };
        }
    }
}