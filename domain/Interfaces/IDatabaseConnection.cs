using domain.Enums;

namespace domain.Interfaces;

public interface IDatabaseConnection
{
    Task Open();
    Task Close();
    Task<bool> TestConnection();
    Task<object> GetSchemaAsync();
    DatabaseType DatabaseType { get; }
    string ConnectionString { get; set; }
    Task<List<Dictionary<string, object>>> ExecuteQuery(string query);
}