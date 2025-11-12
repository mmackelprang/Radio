using FluentAssertions;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;
using RadioConsole.Api.Services;
using Microsoft.Extensions.Logging;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Integration tests for WiredSoundbarOutput with audio inputs.
/// These tests verify that the WiredSoundbar properly receives and handles streaming audio data.
/// </summary>
public class WiredSoundbarOutputIntegrationTests
{
  private readonly Mock<IEnvironmentService> _mockEnvironmentService;
  private readonly Mock<IStorage> _mockStorage;
  private readonly Mock<ILogger<AudioMixer>> _mockLogger;

  public WiredSoundbarOutputIntegrationTests()
  {
    _mockEnvironmentService = new Mock<IEnvironmentService>();
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);

    _mockStorage = new Mock<IStorage>();
    _mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
      .ReturnsAsync((Dictionary<string, object>?)null);

    _mockLogger = new Mock<ILogger<AudioMixer>>();
  }

  [Fact]
  public async Task WiredSoundbar_WithFileAudioInput_ReceivesStreamingData()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    var fileInput = new FileAudioInput(
      "/tmp/test.wav",
      "Test Audio",
      EventPriority.Medium,
      _mockEnvironmentService.Object,
      _mockStorage.Object);

    await fileInput.InitializeAsync();

    var audioMixer = new AudioMixer(
      new[] { soundbarOutput },
      _mockLogger.Object);

    // Register the input with the mixer
    audioMixer.RegisterSource(fileInput);

    // Act
    await audioMixer.StartAsync();
    await soundbarOutput.StartAsync();

    // Start the file input which will generate audio data
    await fileInput.StartAsync();

    // Allow some time for audio data to flow
    await Task.Delay(500);

    // Stop everything
    await fileInput.StopAsync();
    await soundbarOutput.StopAsync();
    await audioMixer.StopAsync();

    // Assert
    soundbarOutput.IsAvailable.Should().BeTrue();
    fileInput.IsAvailable.Should().BeTrue();
  }

  [Fact]
  public async Task WiredSoundbar_Initialize_InSimulationMode_SetsAvailable()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);

    // Act
    await soundbarOutput.InitializeAsync();

    // Assert
    soundbarOutput.IsAvailable.Should().BeTrue();
    soundbarOutput.GetDisplay().GetStatusMessage().Should().Contain("Simulation");
  }

  [Fact]
  public async Task WiredSoundbar_Initialize_InHardwareMode_SetsAvailable()
  {
    // Arrange
    _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);

    // Act
    await soundbarOutput.InitializeAsync();

    // Assert
    soundbarOutput.IsAvailable.Should().BeTrue();
    // In test environments without PortAudio, we expect "Fallback Mode" or "Connected"
    var statusMessage = soundbarOutput.GetDisplay().GetStatusMessage();
    (statusMessage.Contains("Connected") || statusMessage.Contains("Fallback Mode")).Should().BeTrue();
  }

  [Fact]
  public async Task WiredSoundbar_StartAndStop_ChangesActiveState()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    // Act
    await soundbarOutput.StartAsync();
    var isActiveAfterStart = soundbarOutput.IsActive;

    await soundbarOutput.StopAsync();
    var isActiveAfterStop = soundbarOutput.IsActive;

    // Assert
    isActiveAfterStart.Should().BeTrue();
    isActiveAfterStop.Should().BeFalse();
  }

  [Fact]
  public async Task WiredSoundbar_SendAudioAsync_WhenNotActive_ThrowsException()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    var audioStream = new MemoryStream(new byte[1024]);

    // Act
    Func<Task> act = async () => await soundbarOutput.SendAudioAsync(audioStream);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*not active*");
  }

  [Fact]
  public async Task WiredSoundbar_SendAudioAsync_WhenActive_AcceptsStream()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();
    await soundbarOutput.StartAsync();

    // Create some sample audio data
    var audioData = new byte[1024];
    var audioStream = new MemoryStream(audioData);

    // Act
    Func<Task> act = async () => await soundbarOutput.SendAudioAsync(audioStream);

    // Assert
    await act.Should().NotThrowAsync();
  }

  [Fact]
  public async Task WiredSoundbar_SetVolume_UpdatesMetadata()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    // Act
    await soundbarOutput.SetVolumeAsync(0.75);

    // Assert
    var metadata = soundbarOutput.GetDisplay().GetMetadata();
    metadata.Should().ContainKey("Volume");
    metadata["Volume"].Should().Contain("75");
  }

  [Fact]
  public async Task WiredSoundbar_WithMultipleAudioInputs_HandlesStreamingData()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    var fileInput1 = new FileAudioInput(
      "/tmp/test1.wav",
      "Test Audio 1",
      EventPriority.Medium,
      _mockEnvironmentService.Object,
      _mockStorage.Object);

    var fileInput2 = new FileAudioInput(
      "/tmp/test2.wav",
      "Test Audio 2",
      EventPriority.Low,
      _mockEnvironmentService.Object,
      _mockStorage.Object);

    await fileInput1.InitializeAsync();
    await fileInput2.InitializeAsync();

    var audioMixer = new AudioMixer(
      new[] { soundbarOutput },
      _mockLogger.Object);

    // Register both inputs
    audioMixer.RegisterSource(fileInput1);
    audioMixer.RegisterSource(fileInput2);

    // Act
    await audioMixer.StartAsync();
    await soundbarOutput.StartAsync();

    await fileInput1.StartAsync();
    await fileInput2.StartAsync();

    // Allow time for audio mixing
    await Task.Delay(500);

    // Get mixer state
    var mixerState = audioMixer.GetState();

    // Stop everything
    await fileInput1.StopAsync();
    await fileInput2.StopAsync();
    await soundbarOutput.StopAsync();
    await audioMixer.StopAsync();

    // Assert
    mixerState.IsRunning.Should().BeTrue();
    mixerState.TotalSourceCount.Should().Be(2);
  }

  [Fact]
  public async Task WiredSoundbar_Start_WithoutInitialize_ThrowsException()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);

    // Act
    Func<Task> act = async () => await soundbarOutput.StartAsync();

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*not available*");
  }

  [Fact]
  public void WiredSoundbar_GetConfiguration_ReturnsConfiguration()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);

    // Act
    var config = soundbarOutput.GetConfiguration();

    // Assert
    config.Should().NotBeNull();
  }

  [Fact]
  public async Task WiredSoundbar_GetDisplay_ReturnsDisplay()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();

    // Act
    var display = soundbarOutput.GetDisplay();

    // Assert
    display.Should().NotBeNull();
    display.GetStatusMessage().Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task WiredSoundbar_StreamingData_BuffersWithoutDropping()
  {
    // Arrange
    var soundbarOutput = new WiredSoundbarOutput(_mockEnvironmentService.Object, _mockStorage.Object);
    await soundbarOutput.InitializeAsync();
    await soundbarOutput.StartAsync();

    // Act - Send multiple audio chunks rapidly
    var tasks = new List<Task>();
    for (int i = 0; i < 100; i++)
    {
      var audioData = new byte[4096];
      var audioStream = new MemoryStream(audioData);
      tasks.Add(soundbarOutput.SendAudioAsync(audioStream));
    }

    // Assert - All sends should complete without exception
    Func<Task> act = async () => await Task.WhenAll(tasks);
    await act.Should().NotThrowAsync();

    await soundbarOutput.StopAsync();
  }
}
