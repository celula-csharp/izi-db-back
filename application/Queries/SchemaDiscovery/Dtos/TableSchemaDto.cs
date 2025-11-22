namespace application.Queries.SchemaDiscovery.SchemaDto;

public class TableSchemaDto
{
    public string Name { get; set; }
    public List<ColumnSchemaDto> Columns { get; set; } = new();
}