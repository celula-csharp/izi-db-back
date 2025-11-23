namespace application.Queries.SchemaDiscovery.Dtos;

public class FieldSchemaDto
{
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Nullable { get; set; }
    public bool IsArray { get; set; }
    public bool IsObject { get; set; }
}