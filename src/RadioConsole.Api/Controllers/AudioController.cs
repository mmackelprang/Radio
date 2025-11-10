using Microsoft.AspNetCore.Mvc;
using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Services;

namespace RadioConsole.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AudioController : ControllerBase
{
    private readonly IEnumerable<IAudioInput> _audioInputs;
    private readonly IEnumerable<IAudioOutput> _audioOutputs;
    private readonly IAudioPriorityManager _priorityManager;
    private readonly ILogger<AudioController> _logger;

    // Current playback state (in a real implementation, this would be managed by a service)
    private static IAudioInput? _currentInput;
    private static IAudioOutput? _currentOutput;
    private static bool _isPlaying;
    private static int _volume = 50;

    public AudioController(
        IEnumerable<IAudioInput> audioInputs,
        IEnumerable<IAudioOutput> audioOutputs,
        IAudioPriorityManager priorityManager,
        ILogger<AudioController> logger)
    {
        _audioInputs = audioInputs;
        _audioOutputs = audioOutputs;
        _priorityManager = priorityManager;
        _logger = logger;
    }

    [HttpGet("inputs")]
    public IActionResult GetInputs()
    {
        var inputs = _audioInputs.Select(i => new
        {
            i.Id,
            i.Name,
            i.Description,
            i.IsAvailable,
            i.IsActive,
            i.InputType
        });

        return Ok(inputs);
    }

    [HttpGet("outputs")]
    public IActionResult GetOutputs()
    {
        var outputs = _audioOutputs.Select(o => new
        {
            o.Id,
            o.Name,
            o.Description,
            o.IsAvailable,
            o.IsActive
        });

        return Ok(outputs);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartPlaybackRequest request)
    {
        try
        {
            var input = _audioInputs.FirstOrDefault(i => i.Id == request.InputId);
            var output = _audioOutputs.FirstOrDefault(o => o.Id == request.OutputId);

            if (input == null || output == null)
            {
                return BadRequest("Invalid input or output ID");
            }

            if (!input.IsAvailable || !output.IsAvailable)
            {
                return BadRequest("Selected input or output is not available");
            }

            // Stop any currently playing audio
            if (_isPlaying)
            {
                await Stop();
            }

            // Start output first
            await output.StartAsync();

            // Then start input
            await input.StartAsync();

            _currentInput = input;
            _currentOutput = output;
            _isPlaying = true;

            _logger.LogInformation("Started playback: {InputName} -> {OutputName}", input.Name, output.Name);

            return Ok(new
            {
                message = "Playback started",
                input = new { input.Id, input.Name },
                output = new { output.Id, output.Name },
                isPlaying = _isPlaying
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting playback");
            return StatusCode(500, "Error starting playback");
        }
    }

    [HttpPost("stop")]
    public async Task<IActionResult> Stop()
    {
        try
        {
            if (_currentInput != null)
            {
                await _currentInput.StopAsync();
            }

            if (_currentOutput != null)
            {
                await _currentOutput.StopAsync();
            }

            _isPlaying = false;

            _logger.LogInformation("Stopped playback");

            return Ok(new
            {
                message = "Playback stopped",
                isPlaying = _isPlaying
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping playback");
            return StatusCode(500, "Error stopping playback");
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            isPlaying = _isPlaying,
            volume = _volume,
            currentInput = _currentInput != null ? new
            {
                _currentInput.Id,
                _currentInput.Name,
                metadata = _currentInput.GetDisplay().GetMetadata(),
                status = _currentInput.GetDisplay().GetStatusMessage()
            } : null,
            currentOutput = _currentOutput != null ? new
            {
                _currentOutput.Id,
                _currentOutput.Name
            } : null
        });
    }

    [HttpPut("volume")]
    public async Task<IActionResult> SetVolume([FromBody] VolumeRequest request)
    {
        try
        {
            if (request.Volume < 0 || request.Volume > 100)
            {
                return BadRequest("Volume must be between 0 and 100");
            }

            _volume = request.Volume;

            if (_currentOutput != null && _isPlaying)
            {
                await _currentOutput.SetVolumeAsync(request.Volume);
            }

            return Ok(new
            {
                message = "Volume updated",
                volume = _volume
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting volume");
            return StatusCode(500, "Error setting volume");
        }
    }

    [HttpGet("priority/state")]
    public async Task<IActionResult> GetPriorityState()
    {
        try
        {
            var state = await _priorityManager.GetStateAsync();
            
            return Ok(new
            {
                isEventPlaying = state.IsEventPlaying,
                currentEvent = state.CurrentEvent != null ? new
                {
                    state.CurrentEvent.Id,
                    state.CurrentEvent.Name,
                    state.CurrentEvent.Priority,
                    state.CurrentEvent.Duration
                } : null,
                registeredEventInputs = state.RegisteredEventInputs,
                volumeReductionLevel = _priorityManager.Config.VolumeReductionLevel,
                muteBackgroundAudio = _priorityManager.Config.MuteBackgroundAudio
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting priority state");
            return StatusCode(500, "Error getting priority state");
        }
    }

    [HttpPost("priority/config")]
    public IActionResult UpdatePriorityConfig([FromBody] AudioPriorityConfigRequest request)
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
                message = "Priority configuration updated",
                volumeReductionLevel = _priorityManager.Config.VolumeReductionLevel,
                muteBackgroundAudio = _priorityManager.Config.MuteBackgroundAudio
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating priority config");
            return StatusCode(500, "Error updating priority config");
        }
    }

    [HttpPost("events/{eventId}/trigger")]
    public async Task<IActionResult> TriggerEvent(string eventId, [FromBody] TriggerEventRequest? request)
    {
        try
        {
            var eventInput = _audioInputs.FirstOrDefault(i => 
                i.Id == eventId && i.InputType == AudioInputType.Event);

            if (eventInput == null)
            {
                return NotFound($"Event input '{eventId}' not found");
            }

            if (!eventInput.IsAvailable)
            {
                return BadRequest("Event input is not available");
            }

            // Trigger the event with optional metadata
            var metadata = request?.Metadata ?? new Dictionary<string, string>();
            
            // Use reflection to call SimulateTriggerAsync if available
            var method = eventInput.GetType().GetMethod("SimulateTriggerAsync");
            if (method != null)
            {
                await (Task)method.Invoke(eventInput, new object[] { metadata })!;
                
                _logger.LogInformation("Triggered event: {EventName}", eventInput.Name);

                return Ok(new
                {
                    message = "Event triggered successfully",
                    eventId = eventInput.Id,
                    eventName = eventInput.Name,
                    priority = eventInput.Priority,
                    duration = eventInput.Duration?.TotalSeconds
                });
            }

            return BadRequest("Event input does not support manual triggering");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering event");
            return StatusCode(500, "Error triggering event");
        }
    }
}
