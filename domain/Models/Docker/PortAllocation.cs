namespace domain.Models.Docker;

public class PortAllocation
{
    public int Port { get; set; }
    public bool IsAvailable { get; set; }
    public string? ContainerId { get; set; }
    public DateTime AllocatedAt { get; set; }
}