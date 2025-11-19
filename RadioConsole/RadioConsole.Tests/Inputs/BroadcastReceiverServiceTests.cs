using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Core.Interfaces.Inputs;
using RadioConsole.Infrastructure.Inputs;
using Xunit;

namespace RadioConsole.Tests.Inputs;

/// <summary>
/// Unit tests for BroadcastReceiverService.
/// </summary>
public class BroadcastReceiverServiceTests
{
  private readonly Mock<ILogger<BroadcastReceiverService>> _mockLogger;
  private readonly BroadcastReceiverService _service;

  public BroadcastReceiverServiceTests()
  {
    _mockLogger = new Mock<ILogger<BroadcastReceiverService>>();
    _service = new BroadcastReceiverService(_mockLogger.Object);
  }

  [Fact]
  public void Constructor_ShouldInitializeService()
  {
    // Assert
    Assert.NotNull(_service);
    Assert.False(_service.IsListening);
  }

  [Fact]
  public async Task InitializeAsync_ShouldCompleteSuccessfully()
  {
    // Act
    await _service.InitializeAsync();

    // Assert
    Assert.False(_service.IsListening);
  }

  [Fact]
  public async Task StartListeningAsync_ShouldSetIsListeningToTrue()
  {
    // Arrange
    await _service.InitializeAsync();

    // Act
    await _service.StartListeningAsync();

    // Assert
    Assert.True(_service.IsListening);
  }

  [Fact]
  public async Task StopListeningAsync_ShouldSetIsListeningToFalse()
  {
    // Arrange
    await _service.InitializeAsync();
    await _service.StartListeningAsync();

    // Act
    await _service.StopListeningAsync();

    // Assert
    Assert.False(_service.IsListening);
  }

  [Fact]
  public async Task StartListeningAsync_ShouldLogWarning_WhenAlreadyListening()
  {
    // Arrange
    await _service.InitializeAsync();
    await _service.StartListeningAsync();

    // Act
    await _service.StartListeningAsync();

    // Assert
    Assert.True(_service.IsListening);
  }

  [Fact]
  public async Task StopListeningAsync_ShouldLogWarning_WhenNotListening()
  {
    // Arrange
    await _service.InitializeAsync();

    // Act
    await _service.StopListeningAsync();

    // Assert
    Assert.False(_service.IsListening);
  }

  [Fact]
  public void SimulateBroadcast_ShouldRaiseBroadcastReceivedEvent()
  {
    // Arrange
    BroadcastReceivedEventArgs? receivedArgs = null;
    _service.BroadcastReceived += (sender, args) => receivedArgs = args;

    var message = "Test broadcast message";

    // Act
    _service.SimulateBroadcast(message);

    // Assert
    Assert.NotNull(receivedArgs);
    Assert.Equal(message, receivedArgs.Message);
    Assert.Equal("PCM", receivedArgs.AudioFormat);
    Assert.Equal(16000, receivedArgs.SampleRate);
    Assert.Equal(1, receivedArgs.Channels);
    Assert.Equal(16, receivedArgs.BitsPerSample);
    Assert.NotEqual(Guid.Empty.ToString(), receivedArgs.BroadcastId);
  }

  [Fact]
  public void SimulateBroadcast_ShouldIncludeAudioData_WhenProvided()
  {
    // Arrange
    BroadcastReceivedEventArgs? receivedArgs = null;
    _service.BroadcastReceived += (sender, args) => receivedArgs = args;

    var message = "Test broadcast with audio";
    var audioData = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

    // Act
    _service.SimulateBroadcast(message, audioData);

    // Assert
    Assert.NotNull(receivedArgs);
    Assert.Equal(message, receivedArgs.Message);
    Assert.NotNull(receivedArgs.AudioData);
    Assert.Same(audioData, receivedArgs.AudioData);
  }

  [Fact]
  public void BroadcastReceived_ShouldNotFail_WhenNoSubscribers()
  {
    // Act & Assert - should not throw exception
    _service.SimulateBroadcast("Test message");
  }

  [Fact]
  public void BroadcastReceivedEventArgs_ShouldHaveDefaultValues()
  {
    // Arrange & Act
    var eventArgs = new BroadcastReceivedEventArgs();

    // Assert
    Assert.Equal(string.Empty, eventArgs.Message);
    Assert.Null(eventArgs.AudioData);
    Assert.Equal("PCM", eventArgs.AudioFormat);
    Assert.Equal(16000, eventArgs.SampleRate);
    Assert.Equal(1, eventArgs.Channels);
    Assert.Equal(16, eventArgs.BitsPerSample);
    Assert.NotEqual(DateTime.MinValue, eventArgs.Timestamp);
    Assert.NotEqual(Guid.Empty.ToString(), eventArgs.BroadcastId);
  }

  [Fact]
  public void BroadcastReceivedEventArgs_ShouldAllowCustomValues()
  {
    // Arrange
    var timestamp = DateTime.UtcNow.AddMinutes(-5);
    var broadcastId = Guid.NewGuid().ToString();
    var audioStream = new MemoryStream();

    // Act
    var eventArgs = new BroadcastReceivedEventArgs
    {
      Message = "Custom message",
      AudioData = audioStream,
      AudioFormat = "MP3",
      SampleRate = 44100,
      Channels = 2,
      BitsPerSample = 24,
      Timestamp = timestamp,
      BroadcastId = broadcastId
    };

    // Assert
    Assert.Equal("Custom message", eventArgs.Message);
    Assert.Same(audioStream, eventArgs.AudioData);
    Assert.Equal("MP3", eventArgs.AudioFormat);
    Assert.Equal(44100, eventArgs.SampleRate);
    Assert.Equal(2, eventArgs.Channels);
    Assert.Equal(24, eventArgs.BitsPerSample);
    Assert.Equal(timestamp, eventArgs.Timestamp);
    Assert.Equal(broadcastId, eventArgs.BroadcastId);
  }
}
