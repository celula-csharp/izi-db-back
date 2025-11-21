namespace Application.Instances.Dtos;

public class MyInstanceResponseDto
{
    public int UserId { get; set; }
    public int DatabaseInstanceId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string? DatabaseDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
}