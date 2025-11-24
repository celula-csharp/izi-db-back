namespace application.Queries.SchemaDiscovery.Dtos;

public class RedisKeySchemaDto
{
    public string Key { get; set; }
    public string Type { get; set; }
    public List<ColumnSchemaDto> Fields { get; set; } = new();
}