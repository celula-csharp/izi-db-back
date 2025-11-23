using domain.Interfaces;
using infrastructure.Connections;

namespace infrastructure.Factory;

public class DatabaseFactory : IDatabaseFactory
{
    public IDatabaseConnection Create(string engine, string connectionString)
    {
        return engine.ToLower() switch
        {
            "sqlserver" => new SqlServerConnection(connectionString),
            "mysql"     => new MySqlConnectionWrapper(connectionString),
            "postgres"  => new PostgresSqlConnection(connectionString),
            "mongodb"   => new MongoDbConnection(connectionString),
            "redis"     => new RedisConnection(connectionString),

            _ => throw new ArgumentException($"Motor '{engine}' no existe en el factory.")
        };
    }
}