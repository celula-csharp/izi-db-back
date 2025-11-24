namespace api.DTOs;

public class CreateContainerRequest
{
    public string Image { get; set; }
    public string DatabaseType { get; set; }
    public string Credentials { get; set; } // e.g., "user:password"
}
