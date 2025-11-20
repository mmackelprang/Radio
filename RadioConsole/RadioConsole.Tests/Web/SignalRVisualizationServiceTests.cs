using Xunit;
using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RadioConsole.Web.Services;
using RadioConsole.Web.Hubs;

namespace RadioConsole.Tests.Web;

public class SignalRVisualizationServiceTests
{
  private readonly Mock<IHubContext<VisualizerHub>> _mockHubContext;
  private readonly Mock<ILogger<SignalRVisualizationService>> _mockLogger;
  private readonly Mock<IHubClients> _mockClients;
  private readonly Mock<IClientProxy> _mockClientProxy;

  public SignalRVisualizationServiceTests()
  {
    _mockHubContext = new Mock<IHubContext<VisualizerHub>>();
    _mockLogger = new Mock<ILogger<SignalRVisualizationService>>();
    _mockClients = new Mock<IHubClients>();
    _mockClientProxy = new Mock<IClientProxy>();

    _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
    _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
  }

  [Fact]
  public void Constructor_ShouldInitialize()
  {
    // Arrange & Act
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);

    // Assert
    Assert.NotNull(service);
  }

  [Fact]
  public async Task SendFFTDataAsync_ShouldBroadcastToAllClients()
  {
    // Arrange
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);
    var fftData = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };

    // Act
    await service.SendFFTDataAsync(fftData);

    // Assert
    _mockClientProxy.Verify(
      c => c.SendCoreAsync(
        "ReceiveFFTData",
        It.Is<object[]>(args => args.Length == 1 && ((float[])args[0]).Length == 4),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task SendFFTDataAsync_WithCancellationToken_ShouldPassTokenToSignalR()
  {
    // Arrange
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);
    var fftData = new float[] { 0.5f, 0.6f };
    var cancellationToken = new CancellationToken();

    // Act
    await service.SendFFTDataAsync(fftData, cancellationToken);

    // Assert
    _mockClientProxy.Verify(
      c => c.SendCoreAsync(
        "ReceiveFFTData",
        It.IsAny<object[]>(),
        cancellationToken),
      Times.Once);
  }

  [Fact]
  public async Task SendFFTDataAsync_WhenSignalRFails_ShouldLogError()
  {
    // Arrange
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);
    var fftData = new float[] { 0.1f, 0.2f };
    var exception = new Exception("SignalR connection lost");

    _mockClientProxy
      .Setup(c => c.SendCoreAsync(
        It.IsAny<string>(),
        It.IsAny<object[]>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(exception);

    // Act
    await service.SendFFTDataAsync(fftData);

    // Assert
    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send FFT data via SignalR")),
        exception,
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }

  [Fact]
  public async Task SendFFTDataAsync_WithEmptyArray_ShouldStillBroadcast()
  {
    // Arrange
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);
    var fftData = new float[] { };

    // Act
    await service.SendFFTDataAsync(fftData);

    // Assert
    _mockClientProxy.Verify(
      c => c.SendCoreAsync(
        "ReceiveFFTData",
        It.Is<object[]>(args => args.Length == 1),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task SendFFTDataAsync_WithLargeArray_ShouldBroadcastSuccessfully()
  {
    // Arrange
    var service = new SignalRVisualizationService(_mockHubContext.Object, _mockLogger.Object);
    var fftData = new float[256];
    for (int i = 0; i < fftData.Length; i++)
    {
      fftData[i] = (float)i / 256f;
    }

    // Act
    await service.SendFFTDataAsync(fftData);

    // Assert
    _mockClientProxy.Verify(
      c => c.SendCoreAsync(
        "ReceiveFFTData",
        It.Is<object[]>(args => args.Length == 1 && ((float[])args[0]).Length == 256),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }
}
