using Microsoft.AspNetCore.Mvc;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Controllers;

/// <summary>
/// Controller for demonstrating event audio functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsExampleController : ControllerBase
{
    private readonly IEnumerable<IAudioInput> _audioInputs;
    private readonly IAudioPriorityManager _priorityManager;
    private readonly ILogger<EventsExampleController> _logger;

    public EventsExampleController(
        IEnumerable<IAudioInput> audioInputs,
        IAudioPriorityManager priorityManager,
        ILogger<EventsExampleController> logger)
    {
        _audioInputs = audioInputs;
        _priorityManager = priorityManager;
        _logger = logger;
    }

    /// <summary>
    /// Example: Simulate a doorbell ring event
    /// </summary>
    /// <remarks>
    /// This demonstrates how a doorbell ring interrupts current audio playback.
    /// The system will:
    /// 1. Save current volume levels
    /// 2. Reduce/mute background audio
    /// 3. Play doorbell chime
    /// 4. Restore original volumes
    /// </remarks>
    [HttpPost("doorbell/ring")]
    public async Task<IActionResult> SimulateDoorbellRing([FromQuery] string location = "Front Door")
    {
        try
        {
            var doorbellInput = _audioInputs
                .OfType<DoorbellEventInput>()
                .FirstOrDefault();

            if (doorbellInput == null)
            {
                return NotFound("Doorbell event input not found");
            }

            if (!doorbellInput.IsAvailable)
            {
                return BadRequest("Doorbell event input is not available");
            }

            // Sanitize location to prevent log injection
            var sanitizedLocation = location.Replace("\n", "").Replace("\r", "");
            _logger.LogInformation("Simulating doorbell ring at {Location}", sanitizedLocation);

            // Trigger the doorbell event
            await doorbellInput.SimulateDoorbellRingAsync(sanitizedLocation);

            return Ok(new
            {
                message = "Doorbell ring simulated successfully",
                location,
                priority = doorbellInput.Priority.ToString(),
                duration = doorbellInput.Duration?.TotalSeconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating doorbell ring");
            return StatusCode(500, new { error = "Error simulating doorbell ring", details = ex.Message });
        }
    }

    /// <summary>
    /// Example: Simulate a reminder event
    /// </summary>
    /// <remarks>
    /// This demonstrates how a reminder notification works with the priority system.
    /// Reminders have medium priority and will:
    /// 1. Interrupt lower priority sounds
    /// 2. Wait if higher priority sounds are playing
    /// 3. Automatically restore volumes after completion
    /// </remarks>
    [HttpPost("reminder/trigger")]
    public async Task<IActionResult> SimulateReminder([FromQuery] string message = "Sample reminder")
    {
        try
        {
            var reminderInput = _audioInputs
                .OfType<ReminderEventInput>()
                .FirstOrDefault();

            if (reminderInput == null)
            {
                return NotFound("Reminder event input not found");
            }

            if (!reminderInput.IsAvailable)
            {
                return BadRequest("Reminder event input is not available");
            }

            // Sanitize message to prevent log injection
            var sanitizedMessage = message.Replace("\n", "").Replace("\r", "");
            _logger.LogInformation("Simulating reminder: {Message}", sanitizedMessage);

            // Trigger the reminder event
            await reminderInput.SimulateReminderAsync(sanitizedMessage);

            return Ok(new
            {
                message = "Reminder simulated successfully",
                reminderMessage = message,
                priority = reminderInput.Priority.ToString(),
                duration = reminderInput.Duration?.TotalSeconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating reminder");
            return StatusCode(500, new { error = "Error simulating reminder", details = ex.Message });
        }
    }

    /// <summary>
    /// Example: Test multiple events with different priorities
    /// </summary>
    /// <remarks>
    /// This demonstrates the priority system by triggering multiple events in sequence.
    /// Higher priority events will interrupt lower priority ones.
    /// </remarks>
    [HttpPost("test/priority")]
    public async Task<IActionResult> TestPrioritySystem()
    {
        try
        {
            var results = new List<object>();

            _logger.LogInformation("Testing priority system with multiple events");

            // Trigger a low priority event first (reminder)
            var reminderInput = _audioInputs.OfType<ReminderEventInput>().FirstOrDefault();
            if (reminderInput?.IsAvailable == true)
            {
                await reminderInput.SimulateReminderAsync("Test reminder");
                results.Add(new { step = 1, eventType = "Reminder", priority = "Medium", status = "Triggered" });
                await Task.Delay(500); // Small delay
            }

            // Trigger a high priority event (doorbell) which should interrupt
            var doorbellInput = _audioInputs.OfType<DoorbellEventInput>().FirstOrDefault();
            if (doorbellInput?.IsAvailable == true)
            {
                await doorbellInput.SimulateDoorbellRingAsync("Test Door");
                results.Add(new { step = 2, eventType = "Doorbell", priority = "High", status = "Triggered (should interrupt)" });
                await Task.Delay(500); // Small delay
            }

            // Check final state
            var state = await _priorityManager.GetStateAsync();

            return Ok(new
            {
                message = "Priority system test completed",
                results,
                finalState = new
                {
                    state.IsEventPlaying,
                    currentEvent = state.CurrentEvent?.Name,
                    registeredEvents = state.RegisteredEventInputs.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing priority system");
            return StatusCode(500, new { error = "Error testing priority system", details = ex.Message });
        }
    }

    /// <summary>
    /// Example: Hello World - Announce text using TTS
    /// </summary>
    /// <remarks>
    /// This demonstrates the TtsAudioInput with a simple "Hello World" announcement.
    /// The system will:
    /// 1. Generate speech from text using eSpeak TTS
    /// 2. Save current volume levels
    /// 3. Reduce/mute background audio
    /// 4. Play the announcement
    /// 5. Restore original volumes
    /// </remarks>
    [HttpPost("text/announce")]
    public async Task<IActionResult> AnnounceText([FromQuery] string text = "Hello World")
    {
        try
        {
            var textInput = _audioInputs
                .OfType<TtsAudioInput>()
                .FirstOrDefault();

            if (textInput == null)
            {
                return NotFound("Text event input not found");
            }

            if (!textInput.IsAvailable)
            {
                return BadRequest("Text event input is not available - eSpeak TTS may not be configured");
            }

            // Sanitize text to prevent log injection
            var sanitizedText = text.Replace("\n", " ").Replace("\r", " ");
            _logger.LogInformation("Announcing text: {Text}", sanitizedText);

            // Trigger the text announcement
            await textInput.AnnounceTextAsync(sanitizedText);

            return Ok(new
            {
                message = "Text announcement triggered successfully",
                text,
                priority = textInput.Priority.ToString(),
                estimatedDuration = textInput.Duration?.TotalSeconds,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error announcing text");
            return StatusCode(500, new { error = "Error announcing text", details = ex.Message });
        }
    }

    /// <summary>
    /// Example: Hello World - Simple "Hello World" announcement
    /// </summary>
    /// <remarks>
    /// A convenience endpoint that announces "Hello World" using the TtsAudioInput.
    /// Perfect for testing eSpeak TTS integration.
    /// </remarks>
    [HttpPost("text/helloworld")]
    public async Task<IActionResult> HelloWorld()
    {
        return await AnnounceText("Hello World");
    }

    /// <summary>
    /// Get current configuration of the audio priority manager
    /// </summary>
    [HttpGet("config")]
    public IActionResult GetConfiguration()
    {
        return Ok(new
        {
            volumeReductionLevel = _priorityManager.Config.VolumeReductionLevel,
            muteBackgroundAudio = _priorityManager.Config.MuteBackgroundAudio,
            description = new
            {
                volumeReductionLevel = "Volume level for background audio during events (0.0 = mute, 1.0 = no change)",
                muteBackgroundAudio = "If true, completely mutes background audio instead of reducing volume"
            }
        });
    }

    /// <summary>
    /// Update audio priority manager configuration
    /// </summary>
    [HttpPut("config")]
    public IActionResult UpdateConfiguration([FromBody] AudioPriorityConfigUpdateRequest request)
    {
        try
        {
            if (request.VolumeReductionLevel.HasValue)
            {
                if (request.VolumeReductionLevel < 0 || request.VolumeReductionLevel > 1)
                {
                    return BadRequest("Volume reduction level must be between 0.0 and 1.0");
                }
                _priorityManager.Config.VolumeReductionLevel = request.VolumeReductionLevel.Value;
            }

            if (request.MuteBackgroundAudio.HasValue)
            {
                _priorityManager.Config.MuteBackgroundAudio = request.MuteBackgroundAudio.Value;
            }

            return Ok(new
            {
                message = "Configuration updated successfully",
                volumeReductionLevel = _priorityManager.Config.VolumeReductionLevel,
                muteBackgroundAudio = _priorityManager.Config.MuteBackgroundAudio
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration");
            return StatusCode(500, new { error = "Error updating configuration", details = ex.Message });
        }
    }
}

/// <summary>
/// Request model for updating audio priority configuration
/// </summary>
public class AudioPriorityConfigUpdateRequest
{
    public double? VolumeReductionLevel { get; set; }
    public bool? MuteBackgroundAudio { get; set; }
}
