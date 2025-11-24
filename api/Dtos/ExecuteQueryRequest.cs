namespace api.Dtos;

public class ExecuteQueryRequest
{
    public string Engine { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}