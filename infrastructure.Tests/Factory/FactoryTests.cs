using FluentAssertions;
using infrastructure.Factory;
using domain.Interfaces;

namespace infrastructure.Tests;

public class FactoryTests
{
    [Fact]
    public void Factory_Should_Throw_Error_For_Invalid_Engine()
    {
        IDatabaseFactory factory = new DatabaseFactory();

        Action act = () => factory.Create("motorQueNoExiste", "abc");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid database engine*");
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("postgresql")]
    [InlineData("mongodb")]
    [InlineData("redis")]
    public void Factory_Should_Create_Valid_Connections(string engine)
    {
        IDatabaseFactory factory = new DatabaseFactory();

        var result = factory.Create(engine, "conn");

        result.Should().NotBeNull();
    }
}