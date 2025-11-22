using application.Queries.SchemaDiscovery.SchemaDto;
using MongoDB.Bson;

namespace application.Queries.SchemaDiscovery;

public class MongoSchemaDiscovery
{
    private FieldSchemaDto ConvertMongoField(BsonElement element)
    {
        var type = element.Value.BsonType;

        return new FieldSchemaDto
        {
            Name = element.Name,
            Type = type.ToString(),
            Nullable = true,
            IsArray = (type == BsonType.Array),
            IsObject = (type == BsonType.Document)
        };
    }
}