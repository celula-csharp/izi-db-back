using System.Data.Common;
using domain.Enums;
using application.Queries.SchemaDiscovery.SchemaDto;
using application.Dtos;
using infrastructure.Factory;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using StackExchange.Redis;

namespace application.Queries.SchemaDiscovery;

public class DatabaseSchemaDiscovery
{
    private readonly DatabaseFactory _factory;

    public DatabaseSchemaDiscovery(DatabaseFactory factory)
    {
        _factory = factory;
    }

    public async Task<SchemaDto> GetSchemaAsync(DatabaseType engine, string connectionString)
    {
        switch(engine)
        {
            case DatabaseType.SqlServer:
            case DatabaseType.MySql:
            case DatabaseType.PostgreSql:
                return await GetSqlSchemaAsync(connectionString);

            case DatabaseType.MongoDb:
                return await GetMongoSchemaAsync(connectionString);

            case DatabaseType.Redis:
                return await GetRedisSchemaAsync(connectionString);

            default:
                throw new NotSupportedException("Motor no soportado");
        }
    }
    
    private async Task<SchemaDto> GetSqlSchemaAsync(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString); // o MySqlConnection o SqlConnection
        await connection.OpenAsync();

        var tablesQuery = @"SELECT table_name FROM information_schema.tables WHERE table_schema='public';";

        var tables = new List<TableSchemaDto>();

        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = tablesQuery;

            using var reader = await cmd.ExecuteReaderAsync();
            while(await reader.ReadAsync())
            {
                tables.Add(new TableSchemaDto
                {
                    Name = reader.GetString(0)
                });
            }
        }

        foreach (var table in tables)
        {
            table.Columns = await GetSqlColumnsAsync(connection, table.Name);
        }

        return new SchemaDto.SchemaDto { Tables = tables };
    }
    
    private async Task<List<ColumnSchemaDto>> GetSqlColumnsAsync(DbConnection connection, string table)
    {
        var columns = new List<ColumnSchemaDto>();
        var query = $@"
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_name = '{table}';
    ";

        using var cmd = connection.CreateCommand();
        cmd.CommandText = query;

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add(new ColumnSchemaDto
            {
                Name = reader.GetString(0),
                Type = reader.GetString(1),
                Nullable = reader.GetString(2) == "YES"
            });
        }

        return columns;
    }
    
    private async Task<SchemaDto.SchemaDto> GetMongoSchemaAsync(string connectionString)
    {
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase("your-db-name");

        var collections = await db.ListCollectionsAsync();
        var list = await collections.ToListAsync();

        var tables = new List<TableSchemaDto>();

        foreach (var col in list)
        {
            var name = col["name"].AsString;

            var firstDoc = await db.GetCollection<BsonDocument>(name)
                .Find(FilterDefinition<BsonDocument>.Empty)
                .FirstOrDefaultAsync();

            var tableSchema = new TableSchemaDto
            {
                Name = name,
                Columns = new List<ColumnSchemaDto>()
            };

            if (firstDoc != null)
            {
                foreach (var element in firstDoc.Elements)
                {
                    tableSchema.Columns.Add(new ColumnSchemaDto
                    {
                        Name = element.Name,
                        Type = element.Value.BsonType.ToString(),
                        Nullable = true
                    });
                }
            }

            tables.Add(tableSchema);
        }

        return new SchemaDto { Tables = tables };
    }

    private async Task<SchemaDto> GetRedisSchemaAsync(string connectionString)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var db = redis.GetDatabase();

        var server = redis.GetServer(connectionString);
        var keys = server.Keys();

        var tables = new List<TableSchemaDto>();

        foreach (var key in keys)
        {
            var type = await db.KeyTypeAsync(key);

            tables.Add(new TableSchemaDto
            {
                Name = key,
                Columns = new List<ColumnSchemaDto>
                {
                    new ColumnSchemaDto
                    {
                        Name = "value",
                        Type = type.ToString(),
                        Nullable = true
                    }
                }
            });
        }

        return new SchemaDto { Tables = tables };
    }

}