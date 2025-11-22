namespace api.DTOs;

public class QueryExecuteRequest
{
    public int InstanceId { get; set; }
    public string Query { get; set; } = string.Empty;
}