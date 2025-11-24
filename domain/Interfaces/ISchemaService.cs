namespace domain.Interfaces;

public interface ISchemaService
{
    Task<Dictionary<string, List<Dictionary<string, object>>>?> GetSchemaAsync(
        string engine,
        string connectionString);
}