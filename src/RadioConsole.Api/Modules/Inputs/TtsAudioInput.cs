using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Modules.Inputs;

/// <summary>
/// Audio input for text-to-speech announcements using TTS service
/// </summary>
public class TtsAudioInput : BaseAudioInput
{
    public override AudioInputType InputType => AudioInputType.Event;
    private readonly ITtsService _ttsService;
    private string _currentText = string.Empty;
    private Stream? _currentAudioStream;

    public override string Id => "tts_audio";
    public override string Name => "Text-to-Speech";
    public override string Description => "Text-to-speech audio using TTS service";
    public override EventPriority Priority => EventPriority.Medium;
    
    // Duration is dynamic based on text length
    public override TimeSpan? Duration => 
        string.IsNullOrEmpty(_currentText) 
            ? TimeSpan.FromSeconds(5) 
            : TimeSpan.FromSeconds(_ttsService.EstimateDuration(_currentText));

    public TtsAudioInput(
        IEnvironmentService environmentService, 
        IStorage storage,
        ITtsService ttsService)
        : base(environmentService, storage)
    {
        _ttsService = ttsService;
    }

    public override async Task InitializeAsync()
    {
        await _configuration.LoadAsync();
        
        // Initialize TTS service
        await _ttsService.InitializeAsync();
        
        IsAvailable = _ttsService.IsAvailable;
        
        if (IsAvailable)
        {
            _display.UpdateStatus(_environmentService.IsSimulationMode 
                ? "Text Event (Simulation Mode)" 
                : "Text Event Ready (TTS)");
        }
        else
        {
            _display.UpdateStatus("Text Event Not Available - TTS not configured");
        }
    }

    public override async Task StartAsync()
    {
        if (!IsAvailable)
            throw new InvalidOperationException("Text event input is not available");

        IsActive = true;
        _display.UpdateStatus($"Playing text announcement: {_currentText}");
        
        if (_environmentService.IsSimulationMode)
        {
            _display.UpdateMetadata(new Dictionary<string, string>
            {
                ["Event"] = "Text Announcement",
                ["Text"] = _currentText,
                ["Time"] = DateTime.Now.ToString("HH:mm:ss"),
                ["Duration"] = Duration?.TotalSeconds.ToString("F1") ?? "N/A"
            });
        }
        
        await Task.CompletedTask;
    }

    public override Task StopAsync()
    {
        IsActive = false;
        _currentAudioStream?.Dispose();
        _currentAudioStream = null;
        _currentText = string.Empty;
        _display.UpdateStatus("Text announcement completed");
        return Task.CompletedTask;
    }

    public override Task<Stream?> GetAudioStreamAsync()
    {
        if (!string.IsNullOrEmpty(_currentText))
        {
            // Return the pre-generated audio stream
            return Task.FromResult(_currentAudioStream);
        }

        // Fallback to empty stream if no text is set
        return Task.FromResult<Stream?>(new MemoryStream());
    }

    /// <summary>
    /// Announce text using TTS
    /// </summary>
    /// <param name="text">Text to announce</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task AnnounceTextAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be empty", nameof(text));
        }

        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Text event input {Name} is not available");
        }

        // Store text and generate audio
        _currentText = text;
        
        try
        {
            // Generate audio stream using TTS
            _currentAudioStream = await _ttsService.GenerateSpeechAsync(text, cancellationToken);

            // Prepare metadata
            var metadata = new Dictionary<string, string>
            {
                ["Event"] = "Text Announcement",
                ["Text"] = text,
                ["Time"] = DateTime.Now.ToString("HH:mm:ss"),
                ["Duration"] = Duration?.TotalSeconds.ToString("F1") ?? "N/A"
            };

            // Trigger the event
            await TriggerAudioEventAsync(metadata);
        }
        catch (Exception ex)
        {
            _currentText = string.Empty;
            _currentAudioStream?.Dispose();
            _currentAudioStream = null;
            throw new InvalidOperationException($"Failed to generate speech: {ex.Message}", ex);
        }
    }
}
