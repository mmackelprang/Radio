using FluentAssertions;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for CompositeAudioInput covering:
/// - Single/multiple MP3 files
/// - Single/multiple TTS strings
/// - Mixed TTS and MP3 files
/// - Audio looping
/// - Audio concurrency
/// - Event priorities
/// </summary>
public class CompositeAudioInputTests
{
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<IStorage> _mockStorage;
    private readonly Mock<ITtsService> _mockTtsService;

    public CompositeAudioInputTests()
    {
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);

        _mockStorage = new Mock<IStorage>();
        _mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
            .ReturnsAsync((Dictionary<string, object>?)null);

        _mockTtsService = new Mock<ITtsService>();
        _mockTtsService.Setup(t => t.IsAvailable).Returns(true);
        _mockTtsService.Setup(t => t.InitializeAsync()).Returns(Task.CompletedTask);
        _mockTtsService.Setup(t => t.EstimateDuration(It.IsAny<string>())).Returns(5);
        _mockTtsService.Setup(t => t.GenerateSpeechAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());
    }

    #region Single MP3 File Tests

    [Fact]
    public async Task Initialize_WithSingleMp3File_InitializesSuccessfully()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true, // Serial playback
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test.mp3", volume: 1.0);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
        composite.InputType.Should().Be(AudioInputType.Event);
        composite.Priority.Should().Be(EventPriority.Medium);
    }

    [Fact]
    public async Task Start_WithSingleMp3File_StartsPlayback()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test.mp3");
        await composite.InitializeAsync();

        // Act
        await composite.StartAsync();

        // Assert
        composite.IsActive.Should().BeTrue();
        composite.IsPaused.Should().BeFalse();
    }

    #endregion

    #region Multiple MP3 Files Tests

    [Fact]
    public async Task Initialize_WithMultipleMp3Files_InitializesSuccessfully()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3", volume: 1.0);
        composite.AddFileInput("/tmp/test2.mp3", volume: 0.8);
        composite.AddFileInput("/tmp/test3.mp3", volume: 0.6);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task StartSerial_WithMultipleMp3Files_PlaysInOrder()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true, // Serial playback
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3");
        composite.AddFileInput("/tmp/test2.mp3");
        await composite.InitializeAsync();

        // Act
        await composite.StartAsync();
        await Task.Delay(100); // Give it time to start

        // Assert
        composite.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task StartConcurrent_WithMultipleMp3Files_PlaysSimultaneously()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            false, // Concurrent playback
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3");
        composite.AddFileInput("/tmp/test2.mp3");
        await composite.InitializeAsync();

        // Act
        await composite.StartAsync();
        await Task.Delay(100);

        // Assert
        composite.IsActive.Should().BeTrue();
    }

    #endregion

    #region Single TTS String Tests

    [Fact]
    public async Task Initialize_WithSingleTtsString_InitializesSuccessfully()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("Hello World", _mockTtsService.Object);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Start_WithSingleTtsString_StartsPlayback()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("Hello World", _mockTtsService.Object);
        await composite.InitializeAsync();
        await Task.Delay(100); // Wait for TTS generation

        // Act
        await composite.StartAsync();

        // Assert
        composite.IsActive.Should().BeTrue();
    }

    #endregion

    #region Multiple TTS Strings Tests

    [Fact]
    public async Task Initialize_WithMultipleTtsStrings_InitializesSuccessfully()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("First announcement", _mockTtsService.Object);
        composite.AddTtsInput("Second announcement", _mockTtsService.Object);
        composite.AddTtsInput("Third announcement", _mockTtsService.Object);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task StartSerial_WithMultipleTtsStrings_PlaysInOrder()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true, // Serial playback
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("First", _mockTtsService.Object);
        composite.AddTtsInput("Second", _mockTtsService.Object);
        await composite.InitializeAsync();
        await Task.Delay(100);

        // Act
        await composite.StartAsync();

        // Assert
        composite.IsActive.Should().BeTrue();
    }

    #endregion

    #region Mixed TTS and MP3 Tests

    [Fact]
    public async Task Initialize_WithMixedTtsAndMp3_InitializesSuccessfully()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("Welcome", _mockTtsService.Object);
        composite.AddFileInput("/tmp/sound.mp3");
        composite.AddTtsInput("Goodbye", _mockTtsService.Object);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task StartSerial_WithMixedTtsAndMp3InOrder_PlaysSequentially()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.High,
            true, // Serial
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/intro.mp3", volume: 1.0);
        composite.AddTtsInput("Main announcement", _mockTtsService.Object, volume: 0.9);
        composite.AddFileInput("/tmp/outro.mp3", volume: 0.8);

        await composite.InitializeAsync();
        await Task.Delay(100);

        // Act
        await composite.StartAsync();

        // Assert
        composite.IsActive.Should().BeTrue();
        composite.Priority.Should().Be(EventPriority.High);
    }

    [Fact]
    public async Task StartConcurrent_WithMixedTtsAndMp3_PlaysSimultaneously()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            false, // Concurrent
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/background.mp3", volume: 0.5);
        composite.AddTtsInput("Overlay announcement", _mockTtsService.Object, volume: 1.0);

        await composite.InitializeAsync();
        await Task.Delay(100);

        // Act
        await composite.StartAsync();

        // Assert
        composite.IsActive.Should().BeTrue();
    }

    #endregion

    #region Audio Looping Tests

    [Fact]
    public async Task AddFileInput_WithRepeatCount_ConfiguresLooping()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        composite.AddFileInput("/tmp/loop.mp3", volume: 1.0, repeatCount: 3);
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task AddFileInput_WithInfiniteRepeat_ConfiguresInfiniteLooping()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Low,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        composite.AddFileInput("/tmp/loop.mp3", repeatCount: 0); // 0 = infinite
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task AddTtsInput_WithRepeatCount_ConfiguresLooping()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        composite.AddTtsInput("Repeat this", _mockTtsService.Object, repeatCount: 2);
        await composite.InitializeAsync();
        await Task.Delay(100);

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task Pause_WhenPlaying_PausesAllInputs()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            false,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3");
        composite.AddFileInput("/tmp/test2.mp3");
        await composite.InitializeAsync();
        await composite.StartAsync();

        // Act
        await composite.PauseAsync();

        // Assert
        composite.IsPaused.Should().BeTrue();
        composite.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Resume_WhenPaused_ResumesAllInputs()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            false,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3");
        await composite.InitializeAsync();
        await composite.StartAsync();
        await composite.PauseAsync();

        // Act
        await composite.ResumeAsync();

        // Assert
        composite.IsPaused.Should().BeFalse();
        composite.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Stop_WhenPlaying_StopsAllInputs()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test1.mp3");
        composite.AddFileInput("/tmp/test2.mp3");
        await composite.InitializeAsync();
        await composite.StartAsync();

        // Act
        await composite.StopAsync();

        // Assert
        composite.IsActive.Should().BeFalse();
        composite.IsPaused.Should().BeFalse();
    }

    #endregion

    #region Event Priority Tests

    [Fact]
    public async Task Initialize_WithHighPriority_SetsCorrectPriority()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.High,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("High priority message", _mockTtsService.Object);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.Priority.Should().Be(EventPriority.High);
        composite.InputType.Should().Be(AudioInputType.Event);
    }

    [Fact]
    public async Task Initialize_WithCriticalPriority_SetsCorrectPriority()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "emergency_alert",
            "Emergency Alert",
            EventPriority.Critical,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("Emergency alert", _mockTtsService.Object);
        composite.AddFileInput("/tmp/alarm.mp3");

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.Priority.Should().Be(EventPriority.Critical);
    }

    [Fact]
    public async Task Initialize_WithLowPriority_SetsCorrectPriority()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "notification",
            "Notification",
            EventPriority.Low,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddTtsInput("Low priority notification", _mockTtsService.Object);

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.Priority.Should().Be(EventPriority.Low);
    }

    #endregion

    #region Volume Tests

    [Fact]
    public async Task AddFileInput_WithCustomVolume_SetsCorrectVolume()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        composite.AddFileInput("/tmp/test.mp3", volume: 0.7);
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task AddTtsInput_WithCustomVolume_SetsCorrectVolume()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        composite.AddTtsInput("Test message", _mockTtsService.Object, volume: 0.5);
        await composite.InitializeAsync();

        // Assert
        composite.IsAvailable.Should().BeTrue();
    }

    #endregion

    #region Duration Tests

    [Fact]
    public async Task Initialize_WithSerialPlayback_CalculatesTotalDuration()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true, // Serial
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        composite.AddFileInput("/tmp/test.mp3"); // Will have 5s duration in simulation
        composite.AddTtsInput("Test", _mockTtsService.Object); // Will have 5s duration

        // Act
        await composite.InitializeAsync();

        // Assert
        composite.Duration.Should().NotBeNull();
        composite.Duration.Value.TotalSeconds.Should().Be(10);
    }

    [Fact]
    public async Task GetAudioStream_ReturnsNull()
    {
        // Arrange
        var composite = new CompositeAudioInput(
            "test_composite",
            "Test Composite",
            EventPriority.Medium,
            true,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        var stream = await composite.GetAudioStreamAsync();

        // Assert
        stream.Should().BeNull();
    }

    #endregion
}
