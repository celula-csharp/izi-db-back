namespace domain.Models.Docker;

public class DockerContainer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } =  string.Empty;
    public string Status { get; set; } = string.Empty;
    public int HostPort { get; set; }
    public int ContainerPort { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string State { get; set; } = string.Empty;
    
    // Database engine
    public string DatabaseType { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}