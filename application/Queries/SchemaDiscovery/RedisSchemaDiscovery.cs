using application.Queries.SchemaDiscovery.SchemaDto;
using StackExchange.Redis;

namespace application.Queries.SchemaDiscovery;

public class RedisSchemaDiscovery
{
    public async Task<TableSchemaDto> GetRedisSchemaAsync(string key)
    {
        var type = await _redis.GetDatabase().KeyTypeAsync(key);

        var fields = new List<ColumnSchemaDto>();

        switch (type)
        {
            case RedisType.String:
                fields.Add(new ColumnSchemaDto { Name = "value", Type = "string", Nullable = false });
                break;

            case RedisType.Hash:
                var hash = await db.HashGetAllAsync(key);
                foreach (var entry in hash)
                {
                    fields.Add(new ColumnSchemaDto
                    {
                        Name = entry.Name,
                        Type = "string",
                        Nullable = false
                    });
                }
                break;

            case RedisType.List:
                fields.AddRange(new[]
                {
                    new ColumnSchemaDto { Name = "index", Type = "int" },
                    new ColumnSchemaDto { Name = "value", Type = "string" }
                });
                break;

            case RedisType.Set:
                fields.Add(new ColumnSchemaDto { Name = "value", Type = "string" });
                break;

            case RedisType.SortedSet:
                fields.AddRange(new[]
                {
                    new ColumnSchemaDto { Name = "value", Type = "string" },
                    new ColumnSchemaDto { Name = "score", Type = "double" }
                });
                break;

            case RedisType.Stream:
                fields.AddRange(new[]
                {
                    new ColumnSchemaDto { Name = "id", Type = "string" },
                    new ColumnSchemaDto { Name = "fields", Type = "object" }
                });
                break;
        }

        return new TableSchemaDto
        {
            TableName = key,
            Columns = fields
        };
    }
}