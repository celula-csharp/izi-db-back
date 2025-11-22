using domain.Interfaces;

namespace infrastructure.Factory;

public interface IDatabaseFactory
{
    IDatabaseConnection Create(string engine, string connectionString);
}