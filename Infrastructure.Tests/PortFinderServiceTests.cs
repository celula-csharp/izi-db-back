using infrastructure.Services.Docker;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infrastructure.Tests;

public class PortFinderServiceTests
{
    private readonly Mock<ILogger<PortFinderService>> _loggerMock;
    private readonly PortFinderService _portFinder;

    public PortFinderServiceTests()
    {
        _loggerMock = new Mock<ILogger<PortFinderService>>();
        _portFinder = new PortFinderService(_loggerMock.Object);
    }

    [Fact]
    public void FindAvailablePort_ShouldReturnPortInRange()
    {
        // Act
        int port = _portFinder.FindAvailablePort();
        
        // Assert
        Assert.InRange(port, 14330, 14400);
    }
}