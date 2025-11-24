using FluentAssertions;
using infrastructure.Factory;

namespace infrastructure.Tests;

public class ConnectionsTests
{
    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("mongodb")]
    [InlineData("redis")]
    public async Task Should_Open_And_Close_Connections_Without_Error(string engine)
    {
        IDatabaseFactory factory = new DatabaseFactory();
        var connection = factory.Create(engine, "fake-conn");

        Func<Task> open = async () => await connection.Open();
        Func<Task> close = async () => await connection.Close();

        await open.Should().NotThrowAsync();
        await close.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    public async Task Should_Execute_Select_One_In_Relational_Engines(string engine)
    {
        IDatabaseFactory factory = new DatabaseFactory();
        var connection = factory.Create(engine, "fake-conn");

        await connection.Open();
        var result = await connection.ExecuteQuery("SELECT 1 AS TestValue");

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.First().Should().ContainKey("TestValue");

        await connection.Close();
    }
}