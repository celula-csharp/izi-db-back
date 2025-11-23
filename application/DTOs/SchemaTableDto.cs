namespace application.DTOs;

public class SchemaTableDto
{
    public string Name { get; set; } = string.Empty;
    public List<SchemaColumnDto> Columns { get; set; } = new();
}