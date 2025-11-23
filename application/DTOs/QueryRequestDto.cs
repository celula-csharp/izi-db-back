namespace application.DTOs;

public class QueryRequestDto
{
    public string Engine { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}