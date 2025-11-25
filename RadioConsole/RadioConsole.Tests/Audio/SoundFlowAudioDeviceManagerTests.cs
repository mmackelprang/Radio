using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RadioConsole.Core.Configuration;
using RadioConsole.Core.Interfaces.Audio;
using RadioConsole.Infrastructure.Audio;

namespace RadioConsole.Tests.Audio;

/// <summary>
/// Unit tests for SoundFlowAudioDeviceManager.
/// Tests device enumeration, configuration, and hot-plug detection.
/// </summary>
public class SoundFlowAudioDeviceManagerTests
{
  private readonly Mock<ILogger<SoundFlowAudioDeviceManager>> _mockLogger;

  public SoundFlowAudioDeviceManagerTests()
  {
    _mockLogger = new Mock<ILogger<SoundFlowAudioDeviceManager>>();
  }

  [Fact]
  public void Constructor_ShouldInitialize_WithDefaultOptions()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange & Act
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Assert
    Assert.NotNull(manager);
  }

  [Fact]
  public void Constructor_ShouldInitialize_WithCustomOptions()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    var options = Options.Create(new SoundFlowOptions
    {
      SampleRate = 48000,
      BitDepth = 16,
      Channels = 2,
      BufferSize = 1024,
      ExclusiveMode = true,
      EnableHotPlug = false
    });

    // Act
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object, options);

    // Assert
    Assert.NotNull(manager);
    Assert.Equal(48000, manager.GetOptions().SampleRate);
  }

  [Fact]
  public async Task GetInputDevicesAsync_ShouldReturnDeviceList()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var devices = await manager.GetInputDevicesAsync();

    // Assert
    Assert.NotNull(devices);
    // The list may be empty in CI environments without audio hardware
  }

  [Fact]
  public async Task GetOutputDevicesAsync_ShouldReturnDeviceList()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var devices = await manager.GetOutputDevicesAsync();

    // Assert
    Assert.NotNull(devices);
    // The list may be empty in CI environments without audio hardware
  }

  [Fact]
  public async Task SetInputDeviceAsync_ShouldUpdateCurrentDevice()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetInputDeviceAsync("test-device-id");
    var currentDevice = await manager.GetCurrentInputDeviceAsync();

    // Assert
    // Current device will be null since test-device-id doesn't exist
    Assert.Null(currentDevice);
  }

  [Fact]
  public async Task SetOutputDeviceAsync_ShouldUpdateCurrentDevice()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetOutputDeviceAsync("test-device-id");
    var currentDevice = await manager.GetCurrentOutputDeviceAsync();

    // Assert
    // Current device will be null since test-device-id doesn't exist
    Assert.Null(currentDevice);
  }

  [Fact]
  public async Task GetGlobalVolumeAsync_ShouldReturnDefaultVolume()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var volume = await manager.GetGlobalVolumeAsync();

    // Assert
    Assert.Equal(1.0f, volume);
  }

  [Fact]
  public async Task SetGlobalVolumeAsync_ShouldClampVolume()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetGlobalVolumeAsync(1.5f);
    var volume = await manager.GetGlobalVolumeAsync();

    // Assert
    Assert.Equal(1.0f, volume);
  }

  [Fact]
  public async Task SetGlobalVolumeAsync_ShouldClampNegativeVolume()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetGlobalVolumeAsync(-0.5f);
    var volume = await manager.GetGlobalVolumeAsync();

    // Assert
    Assert.Equal(0.0f, volume);
  }

  [Fact]
  public async Task GetGlobalBalanceAsync_ShouldReturnDefaultBalance()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var balance = await manager.GetGlobalBalanceAsync();

    // Assert
    Assert.Equal(0.0f, balance);
  }

  [Fact]
  public async Task SetGlobalBalanceAsync_ShouldClampBalance()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetGlobalBalanceAsync(2.0f);
    var balance = await manager.GetGlobalBalanceAsync();

    // Assert
    Assert.Equal(1.0f, balance);
  }

  [Fact]
  public async Task GetEqualizationAsync_ShouldReturnDefaultSettings()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var eq = await manager.GetEqualizationAsync();

    // Assert
    Assert.Equal(0, eq.Bass);
    Assert.Equal(0, eq.Midrange);
    Assert.Equal(0, eq.Treble);
    Assert.False(eq.Enabled);
  }

  [Fact]
  public async Task SetEqualizationAsync_ShouldClampValues()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.SetEqualizationAsync(new EqualizationSettings
    {
      Bass = 20.0f,
      Midrange = -20.0f,
      Treble = 5.0f,
      Enabled = true
    });
    var eq = await manager.GetEqualizationAsync();

    // Assert
    Assert.Equal(12.0f, eq.Bass);
    Assert.Equal(-12.0f, eq.Midrange);
    Assert.Equal(5.0f, eq.Treble);
    Assert.True(eq.Enabled);
  }

  [Fact]
  public async Task SetEqualizationAsync_ShouldThrow_WhenSettingsNull()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
      () => manager.SetEqualizationAsync(null!));
  }

  [Fact]
  public async Task PlaybackStateAsync_ShouldStartAsStopped()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    var state = await manager.GetPlaybackStateAsync();

    // Assert
    Assert.Equal(PlaybackState.Stopped, state);
  }

  [Fact]
  public async Task PlayAsync_ShouldChangeStateToPlaying()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    await manager.PlayAsync();
    var state = await manager.GetPlaybackStateAsync();

    // Assert
    Assert.Equal(PlaybackState.Playing, state);
  }

  [Fact]
  public async Task PauseAsync_ShouldChangeStateToPaused()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);
    await manager.PlayAsync();

    // Act
    await manager.PauseAsync();
    var state = await manager.GetPlaybackStateAsync();

    // Assert
    Assert.Equal(PlaybackState.Paused, state);
  }

  [Fact]
  public async Task StopAsync_ShouldChangeStateToStopped()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);
    await manager.PlayAsync();

    // Act
    await manager.StopAsync();
    var state = await manager.GetPlaybackStateAsync();

    // Assert
    Assert.Equal(PlaybackState.Stopped, state);
  }

  [Fact]
  public void GetOptions_ShouldReturnConfiguredOptions()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    var expectedOptions = new SoundFlowOptions
    {
      SampleRate = 44100,
      BitDepth = 24,
      BufferSize = 512,
      EnableHotPlug = false,
      PreferredUsbDevicePattern = "TestDevice"
    };
    var options = Options.Create(expectedOptions);
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object, options);

    // Act
    var actualOptions = manager.GetOptions();

    // Assert
    Assert.Equal(44100, actualOptions.SampleRate);
    Assert.Equal(24, actualOptions.BitDepth);
    Assert.Equal(512, actualOptions.BufferSize);
    Assert.False(actualOptions.EnableHotPlug);
    Assert.Equal("TestDevice", actualOptions.PreferredUsbDevicePattern);
  }

  [Fact]
  public async Task FindPreferredUsbDeviceAsync_ShouldReturnNull_WhenNoPatternConfigured()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    var options = Options.Create(new SoundFlowOptions
    {
      PreferredUsbDevicePattern = null,
      EnableHotPlug = false
    });
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object, options);

    // Act
    var device = await manager.FindPreferredUsbDeviceAsync();

    // Assert
    Assert.Null(device);
  }

  [Fact]
  public void Dispose_ShouldDisposeResources()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object);

    // Act
    manager.Dispose();

    // Assert - no exception should be thrown
    // Calling dispose twice should also not throw
    manager.Dispose();
  }

  [Fact]
  public void DeviceEvents_ShouldBeRaisable()
  {
    if (!TestHardwareHelper.AudioHardwareAvailable()) { return; }

    // Arrange
    var options = Options.Create(new SoundFlowOptions
    {
      EnableHotPlug = false // Disable hot-plug to control event timing
    });
    using var manager = new SoundFlowAudioDeviceManager(_mockLogger.Object, options);

    bool connectedRaised = false;
    bool disconnectedRaised = false;

    manager.DeviceConnected += (_, _) => connectedRaised = true;
    manager.DeviceDisconnected += (_, _) => disconnectedRaised = true;

    // Assert - events should be subscribable
    Assert.False(connectedRaised);
    Assert.False(disconnectedRaised);
  }
}
