using Microsoft.AspNetCore.Mvc;
using RadioConsole.Core.Interfaces.Audio;

namespace RadioConsole.API.Controllers;

/// <summary>
/// API controller for system testing and event generation.
/// Provides endpoints to trigger TTS, test tones, and simulated events.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
  private readonly ISystemTestService _testService;
  private readonly ILogger<TestController> _logger;

  public TestController(ISystemTestService testService, ILogger<TestController> logger)
  {
    _testService = testService ?? throw new ArgumentNullException(nameof(testService));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  /// <summary>
  /// Trigger a text-to-speech test.
  /// </summary>
  /// <param name="request">TTS request containing phrase and optional voice parameters.</param>
  /// <returns>200 OK if successful, 400 if invalid request, 500 if error occurs.</returns>
  [HttpPost("tts")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TriggerTts([FromBody] TtsRequest request)
  {
    if (string.IsNullOrWhiteSpace(request?.Phrase))
    {
      return BadRequest(new { error = "Phrase is required" });
    }

    if (_testService.IsTestRunning)
    {
      return Conflict(new { error = "A test is already running. Please wait." });
    }

    try
    {
      _logger.LogInformation("TTS test triggered via API: {Phrase}", request.Phrase);
      await _testService.TriggerTtsAsync(request.Phrase, request.VoiceGender, request.Speed);
      return Ok(new { message = "TTS test completed successfully", phrase = request.Phrase });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error triggering TTS test");
      return StatusCode(500, new { error = "Failed to trigger TTS test", details = ex.Message });
    }
  }

  /// <summary>
  /// Generate and play a test tone.
  /// </summary>
  /// <param name="request">Test tone request containing frequency and duration.</param>
  /// <returns>200 OK if successful, 400 if invalid request, 500 if error occurs.</returns>
  [HttpPost("tone")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TriggerTestTone([FromBody] TestToneRequest? request = null)
  {
    if (_testService.IsTestRunning)
    {
      return Conflict(new { error = "A test is already running. Please wait." });
    }

    var frequency = request?.Frequency ?? 300;
    var duration = request?.DurationSeconds ?? 2;

    if (frequency <= 0 || frequency > 20000)
    {
      return BadRequest(new { error = "Frequency must be between 1 and 20000 Hz" });
    }

    if (duration <= 0 || duration > 10)
    {
      return BadRequest(new { error = "Duration must be between 1 and 10 seconds" });
    }

    try
    {
      _logger.LogInformation("Test tone triggered via API: {Frequency}Hz for {Duration}s", frequency, duration);
      await _testService.TriggerTestToneAsync(frequency, duration);
      return Ok(new { message = "Test tone completed successfully", frequency, duration });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error triggering test tone");
      return StatusCode(500, new { error = "Failed to trigger test tone", details = ex.Message });
    }
  }

  /// <summary>
  /// Simulate a doorbell event.
  /// </summary>
  /// <returns>200 OK if successful, 500 if error occurs.</returns>
  [HttpPost("doorbell")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> TriggerDoorbell()
  {
    if (_testService.IsTestRunning)
    {
      return Conflict(new { error = "A test is already running. Please wait." });
    }

    try
    {
      _logger.LogInformation("Doorbell simulation triggered via API");
      await _testService.TriggerDoorbellAsync();
      return Ok(new { message = "Doorbell simulation completed successfully" });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error triggering doorbell simulation");
      return StatusCode(500, new { error = "Failed to trigger doorbell simulation", details = ex.Message });
    }
  }

  /// <summary>
  /// Get the current test status.
  /// </summary>
  /// <returns>200 OK with test status.</returns>
  [HttpGet("status")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public IActionResult GetStatus()
  {
    return Ok(new { isTestRunning = _testService.IsTestRunning });
  }
}

/// <summary>
/// Request model for TTS endpoint.
/// </summary>
public record TtsRequest
{
  /// <summary>
  /// The text phrase to speak.
  /// </summary>
  public string Phrase { get; init; } = string.Empty;

  /// <summary>
  /// Optional voice gender ("male" or "female").
  /// </summary>
  public string? VoiceGender { get; init; }

  /// <summary>
  /// Optional speech speed (0.5 to 2.0). Default is 1.0.
  /// </summary>
  public float Speed { get; init; } = 1.0f;
}

/// <summary>
/// Request model for test tone endpoint.
/// </summary>
public record TestToneRequest
{
  /// <summary>
  /// Frequency of the tone in Hz. Default is 300Hz.
  /// </summary>
  public int Frequency { get; init; } = 300;

  /// <summary>
  /// Duration of the tone in seconds. Default is 2 seconds.
  /// </summary>
  public int DurationSeconds { get; init; } = 2;
}
