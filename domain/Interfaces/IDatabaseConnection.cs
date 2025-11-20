namespace domain.Interfaces;

public interface IDatabaseConnection
{
    Task Open();
    Task Close();
    Task<string> ExecuteQuery(string query);
}