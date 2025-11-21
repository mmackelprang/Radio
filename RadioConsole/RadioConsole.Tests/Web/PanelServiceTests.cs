using Xunit;
using RadioConsole.Web.Services;

namespace RadioConsole.Tests.Web;

public class PanelServiceTests
{
  [Fact]
  public void Constructor_ShouldInitialize()
  {
    // Arrange & Act
    var service = new PanelService();

    // Assert
    Assert.NotNull(service);
    Assert.Equal(0, service.GetOpenPanelCount());
  }

  [Fact]
  public void IsPanelOpen_WhenPanelNotOpened_ShouldReturnFalse()
  {
    // Arrange
    var service = new PanelService();

    // Act
    var isOpen = service.IsPanelOpen("TestPanel");

    // Assert
    Assert.False(isOpen);
  }

  [Fact]
  public void OpenPanel_ShouldOpenPanel()
  {
    // Arrange
    var service = new PanelService();

    // Act
    service.OpenPanel("TestPanel");

    // Assert
    Assert.True(service.IsPanelOpen("TestPanel"));
    Assert.Equal(1, service.GetOpenPanelCount());
  }

  [Fact]
  public void OpenPanel_ShouldTriggerStateChangedEvent()
  {
    // Arrange
    var service = new PanelService();
    var eventTriggered = false;
    service.OnPanelStateChanged += () => eventTriggered = true;

    // Act
    service.OpenPanel("TestPanel");

    // Assert
    Assert.True(eventTriggered);
  }

  [Fact]
  public void ClosePanel_ShouldCloseOpenPanel()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("TestPanel");

    // Act
    service.ClosePanel("TestPanel");

    // Assert
    Assert.False(service.IsPanelOpen("TestPanel"));
    Assert.Equal(0, service.GetOpenPanelCount());
  }

  [Fact]
  public void ClosePanel_ShouldTriggerStateChangedEvent()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("TestPanel");
    var eventTriggered = false;
    service.OnPanelStateChanged += () => eventTriggered = true;

    // Act
    service.ClosePanel("TestPanel");

    // Assert
    Assert.True(eventTriggered);
  }

  [Fact]
  public void TogglePanel_WhenClosed_ShouldOpen()
  {
    // Arrange
    var service = new PanelService();

    // Act
    service.TogglePanel("TestPanel");

    // Assert
    Assert.True(service.IsPanelOpen("TestPanel"));
  }

  [Fact]
  public void TogglePanel_WhenOpen_ShouldClose()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("TestPanel");

    // Act
    service.TogglePanel("TestPanel");

    // Assert
    Assert.False(service.IsPanelOpen("TestPanel"));
  }

  [Fact]
  public void TogglePanel_ShouldTriggerStateChangedEvent()
  {
    // Arrange
    var service = new PanelService();
    var eventTriggered = false;
    service.OnPanelStateChanged += () => eventTriggered = true;

    // Act
    service.TogglePanel("TestPanel");

    // Assert
    Assert.True(eventTriggered);
  }

  [Fact]
  public void CloseAllPanels_ShouldCloseAllOpenPanels()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("Panel1");
    service.OpenPanel("Panel2");
    service.OpenPanel("Panel3");

    // Act
    service.CloseAllPanels();

    // Assert
    Assert.False(service.IsPanelOpen("Panel1"));
    Assert.False(service.IsPanelOpen("Panel2"));
    Assert.False(service.IsPanelOpen("Panel3"));
    Assert.Equal(0, service.GetOpenPanelCount());
  }

  [Fact]
  public void CloseAllPanels_ShouldTriggerStateChangedEvent()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("Panel1");
    service.OpenPanel("Panel2");
    var eventTriggered = false;
    service.OnPanelStateChanged += () => eventTriggered = true;

    // Act
    service.CloseAllPanels();

    // Assert
    Assert.True(eventTriggered);
  }

  [Fact]
  public void GetOpenPanelCount_ShouldReturnCorrectCount()
  {
    // Arrange
    var service = new PanelService();

    // Act & Assert - Initial state
    Assert.Equal(0, service.GetOpenPanelCount());

    // Act & Assert - Add panels
    service.OpenPanel("Panel1");
    Assert.Equal(1, service.GetOpenPanelCount());

    service.OpenPanel("Panel2");
    Assert.Equal(2, service.GetOpenPanelCount());

    service.OpenPanel("Panel3");
    Assert.Equal(3, service.GetOpenPanelCount());

    // Act & Assert - Close one panel
    service.ClosePanel("Panel2");
    Assert.Equal(2, service.GetOpenPanelCount());

    // Act & Assert - Close all
    service.CloseAllPanels();
    Assert.Equal(0, service.GetOpenPanelCount());
  }

  [Fact]
  public void GetOpenPanels_ShouldReturnListOfOpenPanels()
  {
    // Arrange
    var service = new PanelService();
    service.OpenPanel("Panel1");
    service.OpenPanel("Panel2");
    service.OpenPanel("Panel3");

    // Act
    var openPanels = service.GetOpenPanels();

    // Assert
    Assert.Equal(3, openPanels.Count);
    Assert.Contains("Panel1", openPanels);
    Assert.Contains("Panel2", openPanels);
    Assert.Contains("Panel3", openPanels);
  }

  [Fact]
  public void GetOpenPanels_WhenNoPanelsOpen_ShouldReturnEmptyList()
  {
    // Arrange
    var service = new PanelService();

    // Act
    var openPanels = service.GetOpenPanels();

    // Assert
    Assert.Empty(openPanels);
  }

  [Fact]
  public void MultiplePanels_CanBeOpenSimultaneously()
  {
    // Arrange
    var service = new PanelService();

    // Act
    service.OpenPanel("Configuration");
    service.OpenPanel("SystemStatus");
    service.OpenPanel("AlertManagement");
    service.OpenPanel("SystemTest");

    // Assert
    Assert.True(service.IsPanelOpen("Configuration"));
    Assert.True(service.IsPanelOpen("SystemStatus"));
    Assert.True(service.IsPanelOpen("AlertManagement"));
    Assert.True(service.IsPanelOpen("SystemTest"));
    Assert.Equal(4, service.GetOpenPanelCount());
  }

  [Fact]
  public void OpenPanel_SamePanelTwice_ShouldRemainOpen()
  {
    // Arrange
    var service = new PanelService();

    // Act
    service.OpenPanel("TestPanel");
    service.OpenPanel("TestPanel");

    // Assert
    Assert.True(service.IsPanelOpen("TestPanel"));
    Assert.Equal(1, service.GetOpenPanelCount());
  }

  [Fact]
  public void ClosePanel_NonExistentPanel_ShouldNotThrow()
  {
    // Arrange
    var service = new PanelService();

    // Act & Assert - Should not throw
    service.ClosePanel("NonExistentPanel");
    Assert.False(service.IsPanelOpen("NonExistentPanel"));
  }

  [Fact]
  public void StateChangedEvent_ShouldBeTriggeredOnlyOnStateChanges()
  {
    // Arrange
    var service = new PanelService();
    var eventCount = 0;
    service.OnPanelStateChanged += () => eventCount++;

    // Act
    service.OpenPanel("Panel1");      // +1
    service.ClosePanel("Panel1");     // +1
    service.TogglePanel("Panel2");    // +1
    service.TogglePanel("Panel2");    // +1
    service.CloseAllPanels();         // +1

    // Assert
    Assert.Equal(5, eventCount);
  }

  [Theory]
  [InlineData("Configuration")]
  [InlineData("SystemStatus")]
  [InlineData("AlertManagement")]
  [InlineData("SystemTest")]
  public void PanelService_ShouldHandleExpectedPanelNames(string panelName)
  {
    // Arrange
    var service = new PanelService();

    // Act
    service.OpenPanel(panelName);

    // Assert
    Assert.True(service.IsPanelOpen(panelName));
  }
}
