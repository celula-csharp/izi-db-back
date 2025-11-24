namespace domain.Entities;

public class UserInstance
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int DatabaseInstanceId { get; set; }
    public DatabaseInstance DatabaseInstance { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}