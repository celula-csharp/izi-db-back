using application.DTOs;

namespace application.Interfaces;

public interface ISchemaService
{
    Task<object> GetSchemaAsync(string engine, string connectionString);
}