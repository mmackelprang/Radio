using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RadioConsole.Api.Controllers;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for AudioController covering:
/// - Single/multiple audio inputs
/// - Single/multiple audio outputs
/// - Start/stop/pause/resume/volume
/// - Audio streaming
/// - AudioMixer integration
/// </summary>
public class AudioControllerTests
{
    private readonly Mock<ILogger<AudioController>> _mockLogger;
    private readonly Mock<IAudioPriorityManager> _mockPriorityManager;
    private readonly Mock<ILogger<AudioMixer>> _mockAudioMixerLogger;
    private readonly Mock<IDeviceRegistry> _mockDeviceRegistry;

    public AudioControllerTests()
    {
        _mockLogger = new Mock<ILogger<AudioController>>();
        _mockPriorityManager = new Mock<IAudioPriorityManager>();
        _mockAudioMixerLogger = new Mock<ILogger<AudioMixer>>();
        _mockDeviceRegistry = new Mock<IDeviceRegistry>();
        
        // Setup empty device lists by default
        _mockDeviceRegistry.Setup(r => r.GetAllInputs()).Returns(Array.Empty<IAudioInput>());
        _mockDeviceRegistry.Setup(r => r.GetAllOutputs()).Returns(Array.Empty<IAudioOutput>());
    }

    #region Single Audio Input Tests

    [Fact]
    public void GetInputs_WithSingleInput_ReturnsOneInput()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var inputs = new[] { mockInput.Object };
        var controller = CreateController(inputs);

        // Act
        var result = controller.GetInputs() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var returnedInputs = result.Value as IEnumerable<object>;
        returnedInputs.Should().HaveCount(1);
    }

    [Fact]
    public async Task Start_WithValidSingleInputAndOutput_StartsPlayback()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var inputs = new[] { mockInput.Object };
        var outputs = new[] { mockOutput.Object };
        var controller = CreateController(inputs, outputs);

        var request = new StartPlaybackRequest
        {
            InputId = "input1",
            OutputId = "output1"
        };

        // Act
        var result = await controller.Start(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        mockInput.Verify(i => i.StartAsync(), Times.Once);
        mockOutput.Verify(o => o.StartAsync(), Times.Once);
    }

    [Fact]
    public async Task Start_WithInvalidInputId_ReturnsBadRequest()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var controller = CreateController(new[] { mockInput.Object }, new[] { mockOutput.Object });

        var request = new StartPlaybackRequest
        {
            InputId = "invalid",
            OutputId = "output1"
        };

        // Act
        var result = await controller.Start(request) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Invalid input or output ID");
    }

    [Fact]
    public async Task Start_WithUnavailableInput_ReturnsBadRequest()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", false); // Not available
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var controller = CreateController(new[] { mockInput.Object }, new[] { mockOutput.Object });

        var request = new StartPlaybackRequest
        {
            InputId = "input1",
            OutputId = "output1"
        };

        // Act
        var result = await controller.Start(request) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Selected input or output is not available");
    }

    #endregion

    #region Multiple Audio Inputs Tests

    [Fact]
    public void GetInputs_WithMultipleInputs_ReturnsAllInputs()
    {
        // Arrange
        var mockInput1 = CreateMockAudioInput("input1", "Test Input 1", true);
        var mockInput2 = CreateMockAudioInput("input2", "Test Input 2", true);
        var mockInput3 = CreateMockAudioInput("input3", "Test Input 3", false);
        var inputs = new[] { mockInput1.Object, mockInput2.Object, mockInput3.Object };
        var controller = CreateController(inputs);

        // Act
        var result = controller.GetInputs() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var returnedInputs = result!.Value as IEnumerable<object>;
        returnedInputs.Should().HaveCount(3);
    }

    [Fact]
    public async Task Start_WithMultipleInputs_StartsCorrectInput()
    {
        // Arrange
        var mockInput1 = CreateMockAudioInput("input1", "Test Input 1", true);
        var mockInput2 = CreateMockAudioInput("input2", "Test Input 2", true);
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var inputs = new[] { mockInput1.Object, mockInput2.Object };
        var controller = CreateController(inputs, new[] { mockOutput.Object });

        var request = new StartPlaybackRequest
        {
            InputId = "input2",
            OutputId = "output1"
        };

        // Act
        var result = await controller.Start(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        mockInput1.Verify(i => i.StartAsync(), Times.Never);
        mockInput2.Verify(i => i.StartAsync(), Times.Once);
    }

    #endregion

    #region Single Audio Output Tests

    [Fact]
    public void GetOutputs_WithSingleOutput_ReturnsOneOutput()
    {
        // Arrange
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var outputs = new[] { mockOutput.Object };
        var controller = CreateController(outputs: outputs);

        // Act
        var result = controller.GetOutputs() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var returnedOutputs = result.Value as IEnumerable<object>;
        returnedOutputs.Should().HaveCount(1);
    }

    #endregion

    #region Multiple Audio Outputs Tests

    [Fact]
    public void GetOutputs_WithMultipleOutputs_ReturnsAllOutputs()
    {
        // Arrange
        var mockOutput1 = CreateMockAudioOutput("output1", "Output 1", true);
        var mockOutput2 = CreateMockAudioOutput("output2", "Output 2", true);
        var mockOutput3 = CreateMockAudioOutput("output3", "Output 3", false);
        var outputs = new[] { mockOutput1.Object, mockOutput2.Object, mockOutput3.Object };
        var controller = CreateController(outputs: outputs);

        // Act
        var result = controller.GetOutputs() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        var returnedOutputs = result!.Value as IEnumerable<object>;
        returnedOutputs.Should().HaveCount(3);
    }

    [Fact]
    public async Task Start_WithMultipleOutputs_StartsCorrectOutput()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var mockOutput1 = CreateMockAudioOutput("output1", "Output 1", true);
        var mockOutput2 = CreateMockAudioOutput("output2", "Output 2", true);
        var outputs = new[] { mockOutput1.Object, mockOutput2.Object };
        var controller = CreateController(new[] { mockInput.Object }, outputs);

        var request = new StartPlaybackRequest
        {
            InputId = "input1",
            OutputId = "output2"
        };

        // Act
        var result = await controller.Start(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        mockOutput1.Verify(o => o.StartAsync(), Times.Never);
        mockOutput2.Verify(o => o.StartAsync(), Times.Once);
    }

    #endregion

    #region Start/Stop/Pause/Resume/Volume Tests

    [Fact]
    public async Task Stop_WhenPlaying_StopsPlayback()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var controller = CreateController(new[] { mockInput.Object }, new[] { mockOutput.Object });

        // Start playback first
        await controller.Start(new StartPlaybackRequest { InputId = "input1", OutputId = "output1" });

        // Act
        var result = await controller.Stop() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        mockInput.Verify(i => i.StopAsync(), Times.Once);
        mockOutput.Verify(o => o.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task SetVolume_WithValidVolume_UpdatesVolume()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var controller = CreateController(new[] { mockInput.Object }, new[] { mockOutput.Object });

        // Start playback first
        await controller.Start(new StartPlaybackRequest { InputId = "input1", OutputId = "output1" });

        var volumeRequest = new VolumeRequest { Volume = 75 };

        // Act
        var result = await controller.SetVolume(volumeRequest) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        mockOutput.Verify(o => o.SetVolumeAsync(75), Times.Once);
    }

    [Fact]
    public async Task SetVolume_WithInvalidVolume_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        var volumeRequest = new VolumeRequest { Volume = 150 }; // Invalid

        // Act
        var result = await controller.SetVolume(volumeRequest) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Volume must be between 0 and 100");
    }

    [Fact]
    public async Task SetVolume_WithNegativeVolume_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();
        var volumeRequest = new VolumeRequest { Volume = -10 };

        // Act
        var result = await controller.SetVolume(volumeRequest) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    #endregion

    #region Status Tests

    // Note: GetStatus tests are limited due to static state in AudioController
    // In a production application, this state should be moved to a service

    [Fact]
    public async Task GetStatus_WhenPlaying_ReturnsPlaybackInfo()
    {
        // Arrange
        var mockDisplay = new Mock<IDisplay>();
        mockDisplay.Setup(d => d.GetMetadata()).Returns(new Dictionary<string, string>());
        mockDisplay.Setup(d => d.GetStatusMessage()).Returns("Playing");

        var mockInput = CreateMockAudioInput("input1", "Test Input", true);
        mockInput.Setup(i => i.GetDisplay()).Returns(mockDisplay.Object);

        var mockOutput = CreateMockAudioOutput("output1", "Test Output", true);
        var controller = CreateController(new[] { mockInput.Object }, new[] { mockOutput.Object });

        // Start playback
        await controller.Start(new StartPlaybackRequest { InputId = "input1", OutputId = "output1" });

        // Act
        var result = controller.GetStatus() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    #endregion

    #region Audio Mixer Tests

    [Fact]
    public void GetMixerState_ReturnsState()
    {
        // Arrange
        var audioMixer = new AudioMixer(Array.Empty<IAudioOutput>(), _mockAudioMixerLogger.Object);
        var controller = CreateController(audioMixer: audioMixer);

        // Act
        var result = controller.GetMixerState() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeOfType<AudioMixerState>();
    }

    [Fact]
    public void SetMixerSourceVolume_WithValidVolume_UpdatesVolume()
    {
        // Arrange
        var audioMixer = new AudioMixer(Array.Empty<IAudioOutput>(), _mockAudioMixerLogger.Object);
        var controller = CreateController(audioMixer: audioMixer);
        var volumeRequest = new VolumeRequest { Volume = 80 };

        // Act
        var result = controller.SetMixerSourceVolume("input1", volumeRequest) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public void SetMixerSourceVolume_WithInvalidVolume_ReturnsBadRequest()
    {
        // Arrange
        var audioMixer = new AudioMixer(Array.Empty<IAudioOutput>(), _mockAudioMixerLogger.Object);
        var controller = CreateController(audioMixer: audioMixer);
        var volumeRequest = new VolumeRequest { Volume = 150 };

        // Act
        var result = controller.SetMixerSourceVolume("input1", volumeRequest) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
        result.Value.Should().Be("Volume must be between 0 and 100");
    }

    #endregion

    #region Priority Manager Tests

    [Fact]
    public async Task GetPriorityState_ReturnsState()
    {
        // Arrange
        var mockState = new AudioPriorityState
        {
            IsEventPlaying = false,
            CurrentEvent = null,
            RegisteredEventInputs = new List<string> { "event1", "event2" }
        };

        _mockPriorityManager.Setup(m => m.GetStateAsync()).ReturnsAsync(mockState);
        _mockPriorityManager.Setup(m => m.Config).Returns(new AudioPriorityManagerConfig());

        var controller = CreateController(priorityManager: _mockPriorityManager.Object);

        // Act
        var result = await controller.GetPriorityState() as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public void UpdatePriorityConfig_WithValidConfig_UpdatesConfig()
    {
        // Arrange
        var config = new AudioPriorityManagerConfig
        {
            VolumeReductionLevel = 0.3,
            MuteBackgroundAudio = false
        };

        _mockPriorityManager.Setup(m => m.Config).Returns(config);

        var controller = CreateController(priorityManager: _mockPriorityManager.Object);
        var request = new AudioPriorityConfigRequest
        {
            VolumeReductionLevel = 0.5,
            MuteBackgroundAudio = true
        };

        // Act
        var result = controller.UpdatePriorityConfig(request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        config.VolumeReductionLevel.Should().Be(0.5);
        config.MuteBackgroundAudio.Should().BeTrue();
    }

    [Fact]
    public void UpdatePriorityConfig_WithInvalidVolumeReduction_ReturnsBadRequest()
    {
        // Arrange
        var config = new AudioPriorityManagerConfig();
        _mockPriorityManager.Setup(m => m.Config).Returns(config);

        var controller = CreateController(priorityManager: _mockPriorityManager.Object);
        var request = new AudioPriorityConfigRequest
        {
            VolumeReductionLevel = 1.5 // Invalid
        };

        // Act
        var result = controller.UpdatePriorityConfig(request) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    #endregion

    #region Event Triggering Tests

    [Fact]
    public async Task TriggerEvent_WithValidEventInput_ReturnsOkWhenMethodExists()
    {
        // Arrange
        // We need to use a real event input that has SimulateTriggerAsync method  
        // Since the controller uses reflection, we can't use a mock
        // CompositeAudioInput is an Event type that has SimulateTriggerAsync
        var mockEnvironment = new Mock<IEnvironmentService>();
        mockEnvironment.Setup(e => e.IsSimulationMode).Returns(true);
        var mockStorage = new Mock<IStorage>();
        mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
            .ReturnsAsync((Dictionary<string, object>?)null);
        
        var compositeInput = new CompositeAudioInput(
            "test_event",
            "Test Event",
            EventPriority.High,
            true,
            mockEnvironment.Object,
            mockStorage.Object);
        
        compositeInput.AddFileInput("/tmp/event.mp3");
        await compositeInput.InitializeAsync();
        
        var controller = CreateController(new IAudioInput[] { compositeInput });

        var request = new TriggerEventRequest
        {
            Metadata = new Dictionary<string, string> { ["key"] = "value" }
        };

        // Act
        var result = await controller.TriggerEvent(compositeInput.Id, request) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task TriggerEvent_WithInvalidEventId_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.TriggerEvent("invalid", null) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task TriggerEvent_WithUnavailableEvent_ReturnsBadRequest()
    {
        // Arrange
        var mockInput = CreateMockAudioInput("event1", "Test Event", false, AudioInputType.Event);
        var controller = CreateController(new[] { mockInput.Object });

        // Act
        var result = await controller.TriggerEvent("event1", null) as BadRequestObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(400);
    }

    #endregion

    #region Helper Methods

    private AudioController CreateController(
        IEnumerable<IAudioInput>? inputs = null,
        IEnumerable<IAudioOutput>? outputs = null,
        IAudioPriorityManager? priorityManager = null,
        AudioMixer? audioMixer = null)
    {
        return new AudioController(
            inputs ?? Array.Empty<IAudioInput>(),
            outputs ?? Array.Empty<IAudioOutput>(),
            _mockDeviceRegistry.Object,
            priorityManager ?? _mockPriorityManager.Object,
            audioMixer ?? new AudioMixer(Array.Empty<IAudioOutput>(), _mockAudioMixerLogger.Object),
            _mockLogger.Object);
    }

    private Mock<IAudioInput> CreateMockAudioInput(
        string id,
        string name,
        bool isAvailable,
        AudioInputType inputType = AudioInputType.Music)
    {
        var mock = new Mock<IAudioInput>();
        mock.Setup(i => i.Id).Returns(id);
        mock.Setup(i => i.Name).Returns(name);
        mock.Setup(i => i.Description).Returns($"Description for {name}");
        mock.Setup(i => i.IsAvailable).Returns(isAvailable);
        mock.Setup(i => i.IsActive).Returns(false);
        mock.Setup(i => i.InputType).Returns(inputType);
        mock.Setup(i => i.StartAsync()).Returns(Task.CompletedTask);
        mock.Setup(i => i.StopAsync()).Returns(Task.CompletedTask);
        return mock;
    }

    private Mock<IAudioOutput> CreateMockAudioOutput(string id, string name, bool isAvailable)
    {
        var mock = new Mock<IAudioOutput>();
        mock.Setup(o => o.Id).Returns(id);
        mock.Setup(o => o.Name).Returns(name);
        mock.Setup(o => o.Description).Returns($"Description for {name}");
        mock.Setup(o => o.IsAvailable).Returns(isAvailable);
        mock.Setup(o => o.IsActive).Returns(false);
        mock.Setup(o => o.StartAsync()).Returns(Task.CompletedTask);
        mock.Setup(o => o.StopAsync()).Returns(Task.CompletedTask);
        mock.Setup(o => o.SetVolumeAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
        return mock;
    }

    #endregion
}
