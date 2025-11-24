namespace domain.Models;

public class InstanceInfo
{
    public int Id { get; set; }
    public string Engine { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty; 
}