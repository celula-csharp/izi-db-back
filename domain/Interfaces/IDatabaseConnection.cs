using domain.Enums;

namespace domain.Interfaces;

public interface IDatabaseConnection
{
    Task Open();
    Task Close();
    Task<string> ExecuteQuery(string query);
    Task<bool> TestConnection();
    Task<object> GetSchemaAsync();
    DatabaseType DatabaseType { get; }
    string ConnectionString { get; set; }
}