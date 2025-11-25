using Xunit;
using RadioConsole.Core.Configuration;

namespace RadioConsole.Tests.Configuration;

/// <summary>
/// Unit tests for SoundFlowOptions configuration class.
/// Verifies default values and property behavior.
/// </summary>
public class SoundFlowOptionsTests
{
  [Fact]
  public void DefaultValues_ShouldBeCorrect()
  {
    // Arrange & Act
    var options = new SoundFlowOptions();

    // Assert
    Assert.Equal(48000, options.SampleRate);
    Assert.Equal(16, options.BitDepth);
    Assert.Equal(2, options.Channels);
    Assert.Equal(2048, options.BufferSize);
    Assert.True(options.ExclusiveMode);
    Assert.Equal("alsa", options.PreferredBackend);
    Assert.True(options.EnableHotPlug);
    Assert.Equal(2000, options.HotPlugPollingIntervalMs);
    Assert.Equal("Raddy", options.PreferredUsbDevicePattern);
    Assert.Equal(20, options.LatencyHintMs);
  }

  [Fact]
  public void SampleRate_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.SampleRate = 44100;

    // Assert
    Assert.Equal(44100, options.SampleRate);
  }

  [Fact]
  public void BitDepth_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.BitDepth = 24;

    // Assert
    Assert.Equal(24, options.BitDepth);
  }

  [Fact]
  public void Channels_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.Channels = 1;

    // Assert
    Assert.Equal(1, options.Channels);
  }

  [Fact]
  public void BufferSize_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.BufferSize = 1024;

    // Assert
    Assert.Equal(1024, options.BufferSize);
  }

  [Fact]
  public void ExclusiveMode_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.ExclusiveMode = false;

    // Assert
    Assert.False(options.ExclusiveMode);
  }

  [Fact]
  public void PreferredBackend_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.PreferredBackend = "wasapi";

    // Assert
    Assert.Equal("wasapi", options.PreferredBackend);
  }

  [Fact]
  public void EnableHotPlug_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.EnableHotPlug = false;

    // Assert
    Assert.False(options.EnableHotPlug);
  }

  [Fact]
  public void HotPlugPollingIntervalMs_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.HotPlugPollingIntervalMs = 5000;

    // Assert
    Assert.Equal(5000, options.HotPlugPollingIntervalMs);
  }

  [Fact]
  public void PreferredUsbDevicePattern_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.PreferredUsbDevicePattern = "SH5";

    // Assert
    Assert.Equal("SH5", options.PreferredUsbDevicePattern);
  }

  [Fact]
  public void PreferredUsbDevicePattern_ShouldBeNullable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.PreferredUsbDevicePattern = null;

    // Assert
    Assert.Null(options.PreferredUsbDevicePattern);
  }

  [Fact]
  public void LatencyHintMs_ShouldBeSettable()
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.LatencyHintMs = 10;

    // Assert
    Assert.Equal(10, options.LatencyHintMs);
  }

  [Theory]
  [InlineData(8)]
  [InlineData(16)]
  [InlineData(24)]
  [InlineData(32)]
  public void BitDepth_ShouldAcceptStandardValues(int bitDepth)
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.BitDepth = bitDepth;

    // Assert
    Assert.Equal(bitDepth, options.BitDepth);
  }

  [Theory]
  [InlineData(22050)]
  [InlineData(44100)]
  [InlineData(48000)]
  [InlineData(96000)]
  public void SampleRate_ShouldAcceptStandardValues(int sampleRate)
  {
    // Arrange
    var options = new SoundFlowOptions();

    // Act
    options.SampleRate = sampleRate;

    // Assert
    Assert.Equal(sampleRate, options.SampleRate);
  }
}
