namespace application.Queries.SchemaDiscovery.Dtos;

public class SchemaDto
{
    public List<TableSchemaDto> Tables { get; set; } = new();
}