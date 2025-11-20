using domain.Enums;
using domain.Interfaces;
using infrastructure.Connections;

namespace infrastructure.Factory;

public static class DatabaseFactory
{
    public static IDatabaseConnection Create(DatabaseType type)
    {
        return type switch
        {
            DatabaseType.SqlServer => new SqlServerConnection(),
            DatabaseType.MySql => new MySqlConnection(),
            DatabaseType.PostgreSql => new PostgresSqlConnection(),
            DatabaseType.MongoDb => new MongoDbConnection(),
            DatabaseType.Redis => new RedisConnection(),

            _ => throw new ArgumentException("Motor de base de datos no soportado")
        };
    }
}