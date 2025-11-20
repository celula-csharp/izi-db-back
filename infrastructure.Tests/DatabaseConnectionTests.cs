using FluentAssertions;
using infrastructure.Connections;
using infrastructure.Factory;

namespace infrastructure.Tests;

public class DatabaseConnectionTests
{
    private const string SqlServerConn = "Server=localhost,1433;Database=master;User Id=sa;Password=StrongPass123*;Encrypt=False;";
    private const string MySqlConn = "Server=localhost;Port=3306;Database=testdb;User=adminroot;Password=password;";
    private const string PgConn = "Host=localhost;Port=5432;Database=testdb;Username=adminroot;Password=password;";
    private const string MongoConn = "mongodb://localhost:27017";
    private const string RedisConn = "localhost:6379";

    
    //SQL Server Tests
    [Fact]
    public async Task SqlServer_Should_Open_And_Close_Connection()
    {
        var db = new SqlServerConnection(SqlServerConn);
        
        await db.Open();
        var result = await db.ExecuteQuery("SELECT 1");
        await db.Close();

        result.Should().NotBeNull();
        result.Should().Contain("1"); //valida contenido
    }
    
    // MySQL Test
    [Fact]
    public async Task MySql_Should_Open_And_Execute_Select1()
    {
        var db = new MySqlConnectionWrapper(MySqlConn);
        await db.Open();
        var result = await db.ExecuteQuery("SELECT 1");
        await db.Close();
        
        result.Should().NotBeNull();
        result.Should().Contain("1"); //valida contenido
    }

    // Postgres Test
    [Fact]
    public async Task PostgreSQL_Should_Run_Select1()
    {
        var db = new PostgresSqlConnection(PgConn);
        
        await db.Open();
        var result = await db.ExecuteQuery("SELECT 1");
        await db.Close();
        
        result.Should().NotBeNull();
        result.Should().Contain("1"); //valida contenido
    }
    
    //Mongo Test
    [Fact]
    public async Task Mongo_Should_Connect()
    {
        var db = new MongoDbConnection(MongoConn);
        
        await db.Open();
        var result = await db.ExecuteQuery("""{ "collection": "testCollection", "filter": {} }""");
        await db.Close();
        
        result.Should().NotBeNull();
    }
    
    //Redis Test
    [Fact]
    public async Task Redis_Should_Set_And_Get_Key()
    {
        var db = new RedisConnection(RedisConn);

        await db.Open();

        await db.ExecuteQuery("""{ "command": "set", "key": "test", "value": "123" }""");

        var result = await db.ExecuteQuery("""{ "command": "get", "key": "test" }""");

        await db.Close();

        result.Should().Contain("123");
    }

    
    //Factory error Test
    [Fact]
    public void Factory_Should_Throw_WhenMotorNotExists()
    {
        IDatabaseFactory factory = new DatabaseFactory();

        Action act = () => factory.Create("motorQueNoExiste", "abc");

        act.Should().Throw<ArgumentException>();
    }
}