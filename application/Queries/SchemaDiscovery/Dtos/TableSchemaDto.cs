namespace application.Queries.SchemaDiscovery.Dtos;

public class TableSchemaDto
{
    public string Name { get; set; }
    public List<ColumnSchemaDto> Columns { get; set; } = new();
}