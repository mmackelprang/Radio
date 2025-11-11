using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for DeviceRegistry name uniqueness and duplicate handling
/// </summary>
public class DeviceRegistryTests
{
  private readonly Mock<IStorage> _mockStorage;
  private readonly Mock<IDeviceFactory> _mockDeviceFactory;
  private readonly Mock<ILogger<DeviceRegistry>> _mockLogger;

  public DeviceRegistryTests()
  {
    _mockStorage = new Mock<IStorage>();
    _mockDeviceFactory = new Mock<IDeviceFactory>();
    _mockLogger = new Mock<ILogger<DeviceRegistry>>();
  }

  [Fact]
  public async Task AddInputAsync_WithDuplicateName_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var config1 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice",
      IsEnabled = false
    };

    var config2 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice", // Same name
      IsEnabled = false
    };

    await registry.AddInputAsync(config1);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.AddInputAsync(config2));

    exception.Message.Should().Contain("MyDevice");
    exception.Message.Should().Contain("already exists");
  }

  [Fact]
  public async Task AddOutputAsync_WithDuplicateName_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var config1 = new DeviceConfiguration
    {
      DeviceType = "TestOutput",
      Name = "MyDevice",
      IsEnabled = false
    };

    var config2 = new DeviceConfiguration
    {
      DeviceType = "TestOutput",
      Name = "MyDevice", // Same name
      IsEnabled = false
    };

    await registry.AddOutputAsync(config1);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.AddOutputAsync(config2));

    exception.Message.Should().Contain("MyDevice");
    exception.Message.Should().Contain("already exists");
  }

  [Fact]
  public async Task AddInputAsync_WithSameNameAsOutput_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var outputConfig = new DeviceConfiguration
    {
      DeviceType = "TestOutput",
      Name = "MyDevice",
      IsEnabled = false
    };

    var inputConfig = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice", // Same name as output
      IsEnabled = false
    };

    await registry.AddOutputAsync(outputConfig);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.AddInputAsync(inputConfig));

    exception.Message.Should().Contain("MyDevice");
    exception.Message.Should().Contain("already exists");
  }

  [Fact]
  public async Task AddOutputAsync_WithSameNameAsInput_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var inputConfig = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice",
      IsEnabled = false
    };

    var outputConfig = new DeviceConfiguration
    {
      DeviceType = "TestOutput",
      Name = "MyDevice", // Same name as input
      IsEnabled = false
    };

    await registry.AddInputAsync(inputConfig);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.AddOutputAsync(outputConfig));

    exception.Message.Should().Contain("MyDevice");
    exception.Message.Should().Contain("already exists");
  }

  [Fact]
  public async Task AddInputAsync_WithDifferentName_Succeeds()
  {
    // Arrange
    var registry = CreateRegistry();
    var config1 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "Device1",
      IsEnabled = false
    };

    var config2 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "Device2", // Different name
      IsEnabled = false
    };

    await registry.AddInputAsync(config1);

    // Act
    var result = await registry.AddInputAsync(config2);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be("Device2");
  }

  [Fact]
  public async Task UpdateInputAsync_WithDuplicateName_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var config1 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "Device1",
      IsEnabled = false
    };

    var config2 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "Device2",
      IsEnabled = false
    };

    var added1 = await registry.AddInputAsync(config1);
    var added2 = await registry.AddInputAsync(config2);

    var updateConfig = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "Device1", // Trying to rename to existing name
      IsEnabled = false
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.UpdateInputAsync(added2.Id, updateConfig));

    exception.Message.Should().Contain("Device1");
    exception.Message.Should().Contain("already exists");
  }

  [Fact]
  public async Task UpdateInputAsync_WithSameName_Succeeds()
  {
    // Arrange
    var registry = CreateRegistry();
    var config = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice",
      Description = "Original",
      IsEnabled = false
    };

    var added = await registry.AddInputAsync(config);

    var updateConfig = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice", // Same name is OK for update
      Description = "Updated",
      IsEnabled = false
    };

    // Act
    var result = await registry.UpdateInputAsync(added.Id, updateConfig);

    // Assert
    result.Should().NotBeNull();
    result!.Name.Should().Be("MyDevice");
    result.Description.Should().Be("Updated");
  }

  [Fact]
  public async Task LoadConfigurationsAsync_WithDuplicateInputNames_RemovesDuplicatesAndSaves()
  {
    // Arrange
    var inputConfigs = new List<DeviceConfiguration>
    {
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestInput",
        Name = "Device1",
        IsEnabled = true
      },
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestInput",
        Name = "Device1", // Duplicate name
        IsEnabled = true
      },
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestInput",
        Name = "Device2",
        IsEnabled = true
      }
    };

    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_inputs"))
      .ReturnsAsync(inputConfigs);
    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_outputs"))
      .ReturnsAsync((List<DeviceConfiguration>?)null);
    _mockStorage.Setup(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<object>()))
      .Returns(Task.CompletedTask);

    var mockInput = new Mock<IAudioInput>();
    mockInput.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);
    _mockDeviceFactory.Setup(f => f.CreateInput(It.IsAny<DeviceConfiguration>()))
      .Returns(mockInput.Object);

    var registry = CreateRegistry();

    // Act
    await registry.LoadConfigurationsAsync();

    // Assert
    // Should have saved configurations after detecting duplicates
    _mockStorage.Verify(s => s.SaveAsync("device_registry_inputs", It.IsAny<object>()), Times.Once);
    _mockStorage.Verify(s => s.SaveAsync("device_registry_outputs", It.IsAny<object>()), Times.Once);

    // Should have only 2 unique devices
    var configs = registry.GetAllInputConfigs().ToList();
    configs.Should().HaveCount(2);
    configs.Select(c => c.Name).Should().OnlyHaveUniqueItems();
  }

  [Fact]
  public async Task LoadConfigurationsAsync_WithDuplicateOutputNames_RemovesDuplicatesAndSaves()
  {
    // Arrange
    var outputConfigs = new List<DeviceConfiguration>
    {
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestOutput",
        Name = "Speaker1",
        IsEnabled = true
      },
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestOutput",
        Name = "Speaker1", // Duplicate name
        IsEnabled = true
      }
    };

    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_inputs"))
      .ReturnsAsync((List<DeviceConfiguration>?)null);
    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_outputs"))
      .ReturnsAsync(outputConfigs);
    _mockStorage.Setup(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<object>()))
      .Returns(Task.CompletedTask);

    var mockOutput = new Mock<IAudioOutput>();
    mockOutput.Setup(o => o.InitializeAsync()).Returns(Task.CompletedTask);
    _mockDeviceFactory.Setup(f => f.CreateOutput(It.IsAny<DeviceConfiguration>()))
      .Returns(mockOutput.Object);

    var registry = CreateRegistry();

    // Act
    await registry.LoadConfigurationsAsync();

    // Assert
    // Should have saved configurations after detecting duplicates
    _mockStorage.Verify(s => s.SaveAsync("device_registry_inputs", It.IsAny<object>()), Times.Once);
    _mockStorage.Verify(s => s.SaveAsync("device_registry_outputs", It.IsAny<object>()), Times.Once);

    // Should have only 1 unique device
    var configs = registry.GetAllOutputConfigs().ToList();
    configs.Should().HaveCount(1);
    configs[0].Name.Should().Be("Speaker1");
  }

  [Fact]
  public async Task LoadConfigurationsAsync_WithNoDuplicates_DoesNotSave()
  {
    // Arrange
    var inputConfigs = new List<DeviceConfiguration>
    {
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestInput",
        Name = "Device1",
        IsEnabled = true
      },
      new DeviceConfiguration
      {
        Id = Guid.NewGuid().ToString(),
        DeviceType = "TestInput",
        Name = "Device2",
        IsEnabled = true
      }
    };

    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_inputs"))
      .ReturnsAsync(inputConfigs);
    _mockStorage.Setup(s => s.LoadAsync<List<DeviceConfiguration>>("device_registry_outputs"))
      .ReturnsAsync((List<DeviceConfiguration>?)null);

    var mockInput = new Mock<IAudioInput>();
    mockInput.Setup(i => i.InitializeAsync()).Returns(Task.CompletedTask);
    _mockDeviceFactory.Setup(f => f.CreateInput(It.IsAny<DeviceConfiguration>()))
      .Returns(mockInput.Object);

    var registry = CreateRegistry();

    // Act
    await registry.LoadConfigurationsAsync();

    // Assert
    // Should NOT have saved since there were no duplicates
    _mockStorage.Verify(s => s.SaveAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);

    var configs = registry.GetAllInputConfigs().ToList();
    configs.Should().HaveCount(2);
  }

  [Fact]
  public async Task AddInputAsync_WithCaseInsensitiveDuplicateName_ThrowsInvalidOperationException()
  {
    // Arrange
    var registry = CreateRegistry();
    var config1 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "MyDevice",
      IsEnabled = false
    };

    var config2 = new DeviceConfiguration
    {
      DeviceType = "TestInput",
      Name = "mydevice", // Different case, same name
      IsEnabled = false
    };

    await registry.AddInputAsync(config1);

    // Act & Assert
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await registry.AddInputAsync(config2));

    exception.Message.Should().Contain("already exists");
  }

  private DeviceRegistry CreateRegistry()
  {
    return new DeviceRegistry(_mockStorage.Object, _mockDeviceFactory.Object, _mockLogger.Object);
  }
}
