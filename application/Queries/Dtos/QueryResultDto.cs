namespace application.Queries.Dtos;

public class QueryResultDto
{
    public bool Success { get; set; }
    public List<Dictionary<string, object>>? Records { get; set; }
    public string? Error { get; set; }
}