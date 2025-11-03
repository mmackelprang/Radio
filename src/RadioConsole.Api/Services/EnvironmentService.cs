namespace RadioConsole.Api.Services;

/// <summary>
/// Service to detect the runtime environment and enable simulation mode
/// </summary>
public interface IEnvironmentService
{
    /// <summary>
    /// Whether we're running on a Raspberry Pi
    /// </summary>
    bool IsRaspberryPi { get; }

    /// <summary>
    /// Whether simulation mode is enabled
    /// </summary>
    bool IsSimulationMode { get; }

    /// <summary>
    /// Get the platform description
    /// </summary>
    string PlatformDescription { get; }
}

public class EnvironmentService : IEnvironmentService
{
    public bool IsRaspberryPi { get; }
    public bool IsSimulationMode => !IsRaspberryPi;
    public string PlatformDescription { get; }

    public EnvironmentService()
    {
        // Detect Raspberry Pi by checking for specific files or hardware identifiers
        IsRaspberryPi = DetectRaspberryPi();
        PlatformDescription = GetPlatformDescription();
    }

    private bool DetectRaspberryPi()
    {
        try
        {
            // Check for Raspberry Pi specific files
            if (File.Exists("/proc/device-tree/model"))
            {
                var model = File.ReadAllText("/proc/device-tree/model");
                return model.Contains("Raspberry Pi", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // If we can't read the file, assume we're not on a Raspberry Pi
        }

        return false;
    }

    private string GetPlatformDescription()
    {
        if (IsRaspberryPi)
        {
            try
            {
                if (File.Exists("/proc/device-tree/model"))
                {
                    return File.ReadAllText("/proc/device-tree/model").Replace("\0", "");
                }
            }
            catch
            {
                return "Raspberry Pi (Unknown Model)";
            }
        }

        return $"{Environment.OSVersion.Platform} - Simulation Mode";
    }
}
