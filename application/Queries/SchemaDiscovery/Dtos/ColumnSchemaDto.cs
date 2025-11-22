namespace application.Queries.SchemaDiscovery.SchemaDto;

public class ColumnSchemaDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Nullable { get; set; }
}