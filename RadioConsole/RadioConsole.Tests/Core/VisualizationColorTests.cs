using RadioConsole.Core.Interfaces.Audio;
using Xunit;

namespace RadioConsole.Tests.Interfaces;

/// <summary>
/// Tests for VisualizationColor struct.
/// </summary>
public class VisualizationColorTests
{
  [Fact]
  public void ToHex_WithValidColors_ReturnsCorrectHex()
  {
    // Arrange
    var red = new VisualizationColor(1f, 0f, 0f, 1f);
    var green = new VisualizationColor(0f, 1f, 0f, 1f);
    var blue = new VisualizationColor(0f, 0f, 1f, 1f);

    // Act & Assert
    Assert.Equal("#FF0000FF", red.ToHex());
    Assert.Equal("#00FF00FF", green.ToHex());
    Assert.Equal("#0000FFFF", blue.ToHex());
  }

  [Fact]
  public void ToHex_WithOutOfRangeValues_ClampsCorrectly()
  {
    // Arrange
    var overMax = new VisualizationColor(2f, 2f, 2f, 2f);
    var underMin = new VisualizationColor(-1f, -1f, -1f, -1f);

    // Act & Assert
    Assert.Equal("#FFFFFFFF", overMax.ToHex());
    Assert.Equal("#00000000", underMin.ToHex()); // Alpha is also clamped to 0
  }

  [Fact]
  public void FromHex_WithValidHex_ParsesCorrectly()
  {
    // Act
    var red = VisualizationColor.FromHex("#FF0000");
    var green = VisualizationColor.FromHex("#00FF00FF");

    // Assert
    Assert.Equal(1f, red.R, precision: 2);
    Assert.Equal(0f, red.G, precision: 2);
    Assert.Equal(0f, red.B, precision: 2);
    Assert.Equal(1f, red.A, precision: 2);

    Assert.Equal(0f, green.R, precision: 2);
    Assert.Equal(1f, green.G, precision: 2);
    Assert.Equal(0f, green.B, precision: 2);
    Assert.Equal(1f, green.A, precision: 2);
  }

  [Fact]
  public void FromHex_WithInvalidFormat_ThrowsException()
  {
    // Assert
    Assert.Throws<ArgumentException>(() => VisualizationColor.FromHex("FF0000"));
    Assert.Throws<ArgumentException>(() => VisualizationColor.FromHex("#FFF"));
    Assert.Throws<ArgumentException>(() => VisualizationColor.FromHex(""));
  }

  [Fact]
  public void RoundTrip_ToHexAndFromHex_ReturnsOriginalColor()
  {
    // Arrange
    var original = new VisualizationColor(0.5f, 0.25f, 0.75f, 1f);

    // Act
    var hex = original.ToHex();
    var parsed = VisualizationColor.FromHex(hex);

    // Assert - Allow small precision difference due to float conversion
    Assert.Equal(original.R, parsed.R, precision: 2);
    Assert.Equal(original.G, parsed.G, precision: 2);
    Assert.Equal(original.B, parsed.B, precision: 2);
    Assert.Equal(original.A, parsed.A, precision: 2);
  }

  [Fact]
  public void PredefinedColors_HaveCorrectValues()
  {
    // Get predefined colors
    var red = VisualizationColor.Red;
    var green = VisualizationColor.Green;
    var blue = VisualizationColor.Blue;
    var white = VisualizationColor.White;
    var gray = VisualizationColor.Gray;
    var yellow = VisualizationColor.Yellow;

    // White should be max values
    Assert.Equal(1f, white.R);
    Assert.Equal(1f, white.G);
    Assert.Equal(1f, white.B);
    Assert.Equal(1f, white.A);

    // Verify other colors are not default (all zeros)
    Assert.True(red.R > 0 || red.G > 0 || red.B > 0);
    Assert.True(green.R > 0 || green.G > 0 || green.B > 0);
    Assert.True(blue.R > 0 || blue.G > 0 || blue.B > 0);
  }
}
