using application.Services;
using domain.Interfaces;
using Moq;
using FluentAssertions;
using infrastructure.Factory;

public class SchemaServiceTests
{
    [Fact]
    public async Task Should_Return_Schema_For_SqlServer()
    {
        // Arrange
        var mockFactory = new Mock<IDatabaseFactory>();
        var mockConn = new Mock<IDatabaseConnection>();

        // Configurar conexión simulada
        mockConn.Setup(c => c.Open()).Returns(Task.CompletedTask);
        mockConn.Setup(c => c.Close()).Returns(Task.CompletedTask);

        mockConn.Setup(c => c.ExecuteQuery(It.IsAny<string>()))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "TABLE_NAME", "Users" },
                    { "COLUMN_NAME", "Id" },
                    { "DATA_TYPE", "int" }
                },
                new Dictionary<string, object>
                {
                    { "TABLE_NAME", "Users" },
                    { "COLUMN_NAME", "Name" },
                    { "DATA_TYPE", "varchar" }
                }
            });

        mockFactory
            .Setup(f => f.Create("sqlserver", "conn"))
            .Returns(mockConn.Object);

        var service = new SchemaService(mockFactory.Object);

        // Act
        var result = await service.GetSchemaAsync("sqlserver", "conn");

        // Assert
        result.Should().NotBeNull();

        var success = (bool)result.GetType().GetProperty("success")!.GetValue(result)!;
        success.Should().BeTrue();

        var schema = result.GetType().GetProperty("schema")!.GetValue(result)
                        as Dictionary<string, List<object>>;

        schema.Should().NotBeNull();
        schema!.ContainsKey("Users").Should().BeTrue();
        schema["Users"].Should().HaveCount(2);
    }

    [Fact]
    public async Task Should_Return_Error_When_Engine_Not_Supported()
    {
        // Arrange
        var mockFactory = new Mock<IDatabaseFactory>();
        mockFactory.Setup(f => f.Create("oracle", "123")).Returns((IDatabaseConnection)null);

        var service = new SchemaService(mockFactory.Object);

        // Act
        var result = await service.GetSchemaAsync("oracle", "123");

        // Assert
        var success = (bool)result.GetType().GetProperty("success")!.GetValue(result)!;
        success.Should().BeFalse();

        var error = (string)result.GetType().GetProperty("error")!.GetValue(result)!;
        error.Should().Contain("not supported");
    }
}
