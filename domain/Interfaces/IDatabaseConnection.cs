namespace domain.Interfaces;

public interface IDatabaseConnection
{
    Task Open();
    Task Close();
    Task<List<Dictionary<string, object>>> ExecuteQuery(string query);
}