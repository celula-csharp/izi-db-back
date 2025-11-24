using Moq;
using application.Services;
using domain.Interfaces;
using FluentAssertions;
using infrastructure.Factory;

public class QueryExecutorTests
{
    [Fact]
    public async Task Should_Execute_Query_Correctly()
    {
        // Arrange
        var mockFactory = new Mock<IDatabaseFactory>();
        var mockConn = new Mock<IDatabaseConnection>();

        mockConn.Setup(c => c.Open()).Returns(Task.CompletedTask);
        mockConn.Setup(c => c.Close()).Returns(Task.CompletedTask);

        mockConn.Setup(c => c.ExecuteQuery("SELECT 1"))
            .ReturnsAsync(new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "TestValue", 1 } }
            });

        mockFactory
            .Setup(f => f.Create("sqlserver", "fake-conn"))
            .Returns(mockConn.Object);

        var executor = new QueryExecutor(mockFactory.Object);

        // Act
        var result = await executor.ExecuteQueryAsync(
            "sqlserver",
            "SELECT 1",
            "fake-conn",
            "user1"
        );

        // Assert
        result.Success.Should().BeTrue();
        result.Records.Should().HaveCount(1);
        result.Records[0].Should().ContainKey("TestValue");
        result.Records[0]["TestValue"].Should().Be(1);
    }

    [Fact]
    public async Task Should_Fail_When_Engine_Is_Not_Supported()
    {
        var mockFactory = new Mock<IDatabaseFactory>();
        mockFactory.Setup(f => f.Create("oracle", "123"))
            .Returns((IDatabaseConnection)null);

        var executor = new QueryExecutor(mockFactory.Object);

        var result = await executor.ExecuteQueryAsync(
            "oracle",
            "SELECT 1",
            "123",
            "user1"
        );

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("not supported");
    }
}