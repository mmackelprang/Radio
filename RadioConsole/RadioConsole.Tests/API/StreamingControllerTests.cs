using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.API.Controllers;
using RadioConsole.API.Services;
using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.API;

/// <summary>
/// Unit tests for StreamingController and StreamAudioService.
/// </summary>
public class StreamingControllerTests
{
  [Fact]
  public void StreamAudioService_SupportedFormats_ContainsAllRequiredFormats()
  {
    // Arrange
    var expectedFormats = new[] { "wav", "mp3", "flac", "aac", "ogg", "opus" };

    // Act
    var supportedFormats = StreamAudioService.SupportedFormats;

    // Assert
    Assert.Equal(expectedFormats.Length, supportedFormats.Length);
    foreach (var format in expectedFormats)
    {
      Assert.Contains(format, supportedFormats);
    }
  }

  [Theory]
  [InlineData("wav", true)]
  [InlineData("mp3", true)]
  [InlineData("flac", true)]
  [InlineData("aac", true)]
  [InlineData("ogg", true)]
  [InlineData("opus", true)]
  [InlineData("WAV", true)] // Test case insensitivity
  [InlineData("MP3", true)]
  [InlineData("xyz", false)]
  [InlineData("", false)]
  [InlineData("avi", false)]
  public void StreamAudioService_IsFormatSupported_ReturnsCorrectValue(string format, bool expected)
  {
    // Act
    var isSupported = StreamAudioService.IsFormatSupported(format);

    // Assert
    Assert.Equal(expected, isSupported);
  }

  [Theory]
  [InlineData("wav", "audio/wav")]
  [InlineData("mp3", "audio/mpeg")]
  [InlineData("flac", "audio/flac")]
  [InlineData("aac", "audio/aac")]
  [InlineData("ogg", "audio/ogg")]
  [InlineData("opus", "audio/opus")]
  [InlineData("WAV", "audio/wav")] // Test case insensitivity
  [InlineData("unknown", "audio/mpeg")] // Default
  public void StreamAudioService_GetContentType_ReturnsCorrectMimeType(string format, string expectedContentType)
  {
    // Act
    var contentType = StreamAudioService.GetContentType(format);

    // Assert
    Assert.Equal(expectedContentType, contentType);
  }
}
