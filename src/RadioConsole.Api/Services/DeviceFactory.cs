using RadioConsole.Api.Interfaces;
using RadioConsole.Api.Models;
using RadioConsole.Api.Modules.Inputs;
using RadioConsole.Api.Modules.Outputs;
using System.Text.Json;

namespace RadioConsole.Api.Services;

/// <summary>
/// Factory for creating audio devices dynamically from configuration
/// </summary>
public class DeviceFactory : IDeviceFactory
{
    private readonly IEnvironmentService _environmentService;
    private readonly IStorage _storage;
    private readonly ITtsService _ttsService;
    private readonly ILogger<DeviceFactory> _logger;

    public DeviceFactory(
        IEnvironmentService environmentService,
        IStorage storage,
        ITtsService ttsService,
        ILogger<DeviceFactory> logger)
    {
        _environmentService = environmentService;
        _storage = storage;
        _ttsService = ttsService;
        _logger = logger;
    }

    public IAudioInput CreateInput(DeviceConfiguration config)
    {
        _logger.LogInformation("Creating input device: {Name} of type {Type}", config.Name, config.DeviceType);

        return config.DeviceType switch
        {
            "UsbAudioInput" => CreateUsbAudioInput(config),
            "FileAudioInput" => CreateFileAudioInput(config),
            "TtsAudioInput" => CreateTtsAudioInput(config),
            "SpotifyInput" => CreateSpotifyInput(config),
            _ => throw new ArgumentException($"Unknown input device type: {config.DeviceType}")
        };
    }

    public IAudioOutput CreateOutput(DeviceConfiguration config)
    {
        _logger.LogInformation("Creating output device: {Name} of type {Type}", config.Name, config.DeviceType);

        return config.DeviceType switch
        {
            "WiredSoundbarOutput" => CreateWiredSoundbarOutput(config),
            "ChromecastOutput" => CreateChromecastOutput(config),
            _ => throw new ArgumentException($"Unknown output device type: {config.DeviceType}")
        };
    }

    private IAudioInput CreateUsbAudioInput(DeviceConfiguration config)
    {
        var deviceNumber = GetParameter<int>(config, "DeviceNumber", -1);
        return new UsbAudioInput(deviceNumber, config.Name, config.Description, _environmentService, _storage);
    }

    private IAudioInput CreateFileAudioInput(DeviceConfiguration config)
    {
        var filePath = GetParameter<string>(config, "FilePath", "");
        var priority = GetParameter<string>(config, "Priority", "Medium");
        var repeat = GetParameter<int>(config, "Repeat", -1);
        
        EventPriority eventPriority;
        if (!Enum.TryParse<EventPriority>(priority, true, out eventPriority))
        {
            eventPriority = EventPriority.Medium;
        }
        
        var input = new FileAudioInput(filePath, config.Name, eventPriority, _environmentService, _storage);
        if (repeat >= 0)
        {
            input.SetRepeat(repeat);
        }
        return input;
    }

    private IAudioInput CreateTtsAudioInput(DeviceConfiguration config)
    {
        return new TtsAudioInput(_environmentService, _storage, _ttsService);
    }

    private IAudioInput CreateSpotifyInput(DeviceConfiguration config)
    {
        // Note: SpotifyInput might need additional parameters like credentials
        return new SpotifyInput(_environmentService, _storage);
    }

    private IAudioOutput CreateWiredSoundbarOutput(DeviceConfiguration config)
    {
        return new WiredSoundbarOutput(_environmentService, _storage);
    }

    private IAudioOutput CreateChromecastOutput(DeviceConfiguration config)
    {
        return new ChromecastOutput(_environmentService, _storage);
    }

    private T GetParameter<T>(DeviceConfiguration config, string key, T defaultValue)
    {
        if (config.Parameters.TryGetValue(key, out var value))
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                }
                if (value is T typedValue)
                {
                    return typedValue;
                }
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert parameter {Key} to type {Type}, using default", key, typeof(T).Name);
            }
        }
        return defaultValue;
    }

    public IEnumerable<DeviceTypeInfo> GetAvailableInputTypes()
    {
        return new List<DeviceTypeInfo>
        {
            new()
            {
                TypeName = "UsbAudioInput",
                DisplayName = "USB Audio Device",
                Description = "Captures audio from a USB audio device (radio receiver, turntable, etc.)",
                Category = "Input",
                Parameters = new List<DeviceParameterInfo>
                {
                    new()
                    {
                        Name = "DeviceNumber",
                        DisplayName = "Device Number",
                        Description = "USB audio device number (-1 for default device)",
                        DataType = "int",
                        Required = false,
                        DefaultValue = -1
                    }
                }
            },
            new()
            {
                TypeName = "FileAudioInput",
                DisplayName = "Audio File",
                Description = "Plays audio from a file (MP3, WAV, etc.)",
                Category = "Input",
                Parameters = new List<DeviceParameterInfo>
                {
                    new()
                    {
                        Name = "FilePath",
                        DisplayName = "File Path",
                        Description = "Path to the audio file",
                        DataType = "string",
                        Required = true,
                        DefaultValue = ""
                    },
                    new()
                    {
                        Name = "Priority",
                        DisplayName = "Priority",
                        Description = "Priority level (Low, Medium, High, Critical)",
                        DataType = "string",
                        Required = false,
                        DefaultValue = "Medium"
                    },
                    new()
                    {
                        Name = "Repeat",
                        DisplayName = "Repeat Count",
                        Description = "Number of times to repeat (0 = infinite, -1 = no repeat)",
                        DataType = "int",
                        Required = false,
                        DefaultValue = -1
                    }
                }
            },
            new()
            {
                TypeName = "TtsAudioInput",
                DisplayName = "Text-to-Speech",
                Description = "Converts text to speech using eSpeak",
                Category = "Input",
                Parameters = new List<DeviceParameterInfo>()
            },
            new()
            {
                TypeName = "SpotifyInput",
                DisplayName = "Spotify",
                Description = "Streams audio from Spotify",
                Category = "Input",
                Parameters = new List<DeviceParameterInfo>()
            }
        };
    }

    public IEnumerable<DeviceTypeInfo> GetAvailableOutputTypes()
    {
        return new List<DeviceTypeInfo>
        {
            new()
            {
                TypeName = "WiredSoundbarOutput",
                DisplayName = "Wired Soundbar",
                Description = "Outputs audio to a wired soundbar or speaker system",
                Category = "Output",
                Parameters = new List<DeviceParameterInfo>()
            },
            new()
            {
                TypeName = "ChromecastOutput",
                DisplayName = "Chromecast Device",
                Description = "Streams audio to a Chromecast device",
                Category = "Output",
                Parameters = new List<DeviceParameterInfo>()
            }
        };
    }
}
