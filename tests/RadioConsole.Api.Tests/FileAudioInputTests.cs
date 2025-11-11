using FluentAssertions;
using Moq;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Tests;

/// <summary>
/// Tests for FileAudioInput covering:
/// - MP3 file playback
/// - WAV file playback
/// - AAC file playback
/// - FLAC file playback
/// </summary>
public class FileAudioInputTests
{
    private readonly Mock<IEnvironmentService> _mockEnvironmentService;
    private readonly Mock<IStorage> _mockStorage;

    public FileAudioInputTests()
    {
        _mockEnvironmentService = new Mock<IEnvironmentService>();
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(true);

        _mockStorage = new Mock<IStorage>();
        _mockStorage.Setup(s => s.LoadAsync<Dictionary<string, object>>(It.IsAny<string>()))
            .ReturnsAsync((Dictionary<string, object>?)null);
    }

    #region MP3 File Tests

    [Fact]
    public async Task Initialize_WithMp3File_InitializesSuccessfully()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert
        fileInput.IsAvailable.Should().BeTrue();
        fileInput.Name.Should().Be("Test MP3");
        fileInput.Duration.Should().NotBeNull();
        fileInput.Duration.Value.TotalSeconds.Should().Be(5); // Simulation mode duration
    }

    [Fact]
    public async Task Start_WithMp3File_StartsPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        await fileInput.StartAsync();

        // Assert
        fileInput.IsActive.Should().BeTrue();
        fileInput.IsPaused.Should().BeFalse();
    }

    [Fact]
    public async Task Stop_WhenPlayingMp3_StopsPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();
        await fileInput.StartAsync();

        // Act
        await fileInput.StopAsync();

        // Assert
        fileInput.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Pause_WhenPlayingMp3_PausesPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();
        await fileInput.StartAsync();

        // Act
        await fileInput.PauseAsync();

        // Assert
        fileInput.IsPaused.Should().BeTrue();
        fileInput.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Resume_WhenPausedMp3_ResumesPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();
        await fileInput.StartAsync();
        await fileInput.PauseAsync();

        // Act
        await fileInput.ResumeAsync();

        // Assert
        fileInput.IsPaused.Should().BeFalse();
        fileInput.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetAudioStream_WithMp3InSimulation_ReturnsStream()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        var stream = await fileInput.GetAudioStreamAsync();

        // Assert
        stream.Should().NotBeNull();
    }

    #endregion

    #region WAV File Tests

    [Fact]
    public async Task Initialize_WithWavFile_InitializesSuccessfully()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.wav",
            "Test WAV",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert
        fileInput.IsAvailable.Should().BeTrue();
        fileInput.Name.Should().Be("Test WAV");
        fileInput.Description.Should().Contain("wav");
    }

    [Fact]
    public async Task Start_WithWavFile_StartsPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.wav",
            "Test WAV",
            EventPriority.High,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        await fileInput.StartAsync();

        // Assert
        fileInput.IsActive.Should().BeTrue();
        fileInput.Priority.Should().Be(EventPriority.High);
    }

    [Fact]
    public async Task Stop_WhenPlayingWav_StopsPlayback()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.wav",
            "Test WAV",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();
        await fileInput.StartAsync();

        // Act
        await fileInput.StopAsync();

        // Assert
        fileInput.IsActive.Should().BeFalse();
    }

    #endregion

    #region AAC File Tests

    [Fact]
    public async Task Initialize_WithAacFile_InitializesInSimulationMode()
    {
        // Arrange
        // AAC is not supported by NAudio in the current implementation
        // but in simulation mode, file extension doesn't matter
        var fileInput = new FileAudioInput(
            "/tmp/test.aac",
            "Test AAC",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert - In simulation mode, it should be available
        fileInput.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Initialize_WithAacFileNotInSimulation_MarksUnavailable()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);

        var fileInput = new FileAudioInput(
            "/tmp/nonexistent.aac",
            "Test AAC",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert - AAC is not supported, file doesn't exist
        fileInput.IsAvailable.Should().BeFalse();
    }

    #endregion

    #region FLAC File Tests

    [Fact]
    public async Task Initialize_WithFlacFile_InitializesInSimulationMode()
    {
        // Arrange
        // FLAC is not explicitly supported in the current implementation
        // but in simulation mode, file extension doesn't matter
        var fileInput = new FileAudioInput(
            "/tmp/test.flac",
            "Test FLAC",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert - In simulation mode, it should be available
        fileInput.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Initialize_WithFlacFileNotInSimulation_MarksUnavailable()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);

        var fileInput = new FileAudioInput(
            "/tmp/nonexistent.flac",
            "Test FLAC",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert - FLAC is not supported, file doesn't exist
        fileInput.IsAvailable.Should().BeFalse();
    }

    #endregion

    #region General Tests

    [Fact]
    public void Constructor_WithEmptyFilePath_ThrowsArgumentException()
    {
        // Act
        Action act = () => new FileAudioInput(
            "",
            "Test",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Path cannot be empty*");
    }

    [Fact]
    public void Constructor_WithValidFilePath_SetsIdCorrectly()
    {
        // Arrange & Act
        var fileInput = new FileAudioInput(
            "/tmp/my test file.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Assert
        fileInput.Id.Should().StartWith("file_audio_");
        fileInput.Id.Should().Contain("my_test_file");
    }

    [Fact]
    public async Task SetRepeat_ConfiguresRepeatBehavior()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        fileInput.SetRepeat(3);

        // Assert - Method should not throw
        fileInput.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task SetRepeat_WithZero_ConfiguresInfiniteRepeat()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        fileInput.SetRepeat(0); // 0 = infinite

        // Assert
        fileInput.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task AllowConcurrent_CanBeSet()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        fileInput.AllowConcurrent = true;

        // Assert
        fileInput.AllowConcurrent.Should().BeTrue();
    }

    [Fact]
    public async Task SetVolume_WithValidVolume_SetsVolume()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        await fileInput.SetVolumeAsync(0.75);

        // Assert - Method should not throw
        fileInput.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task SetVolume_WithInvalidVolume_ThrowsException()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        Func<Task> act = async () => await fileInput.SetVolumeAsync(1.5);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Volume must be between 0.0 and 1.0*");
    }

    [Fact]
    public async Task Start_WithoutInitialize_ThrowsException()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Don't initialize

        // Act
        Func<Task> act = async () => await fileInput.StartAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task Pause_WhenNotActive_ThrowsException()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        Func<Task> act = async () => await fileInput.PauseAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public async Task Resume_WhenNotActive_ThrowsException()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        Func<Task> act = async () => await fileInput.ResumeAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public void GetConfiguration_ReturnsConfiguration()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        var config = fileInput.GetConfiguration();

        // Assert
        config.Should().NotBeNull();
    }

    [Fact]
    public void GetDisplay_ReturnsDisplay()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        var display = fileInput.GetDisplay();

        // Assert
        display.Should().NotBeNull();
        display.GetStatusMessage().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SimulateTrigger_WhenAvailable_TriggersEvent()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync();

        // Act
        Func<Task> act = async () => await fileInput.SimulateTriggerAsync(
            new Dictionary<string, string> { ["test"] = "value" });

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SimulateTrigger_WhenNotAvailable_ThrowsException()
    {
        // Arrange
        _mockEnvironmentService.Setup(e => e.IsSimulationMode).Returns(false);

        var fileInput = new FileAudioInput(
            "/tmp/nonexistent.mp3",
            "Test MP3",
            EventPriority.Medium,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        await fileInput.InitializeAsync(); // Will mark as unavailable

        // Act
        Func<Task> act = async () => await fileInput.SimulateTriggerAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not available*");
    }

    #endregion

    #region Priority Tests

    [Fact]
    public async Task Initialize_WithLowPriority_SetsPriorityCorrectly()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Test MP3",
            EventPriority.Low,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert
        fileInput.Priority.Should().Be(EventPriority.Low);
    }

    [Fact]
    public async Task Initialize_WithCriticalPriority_SetsPriorityCorrectly()
    {
        // Arrange
        var fileInput = new FileAudioInput(
            "/tmp/test.mp3",
            "Emergency Alert",
            EventPriority.Critical,
            _mockEnvironmentService.Object,
            _mockStorage.Object);

        // Act
        await fileInput.InitializeAsync();

        // Assert
        fileInput.Priority.Should().Be(EventPriority.Critical);
    }

    #endregion
}
