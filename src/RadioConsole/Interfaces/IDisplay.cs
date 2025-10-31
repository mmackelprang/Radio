namespace RadioConsole.Interfaces;

/// <summary>
/// Interface for displaying metadata and status information
/// </summary>
public interface IDisplay
{
    /// <summary>
    /// Get the current metadata to display
    /// </summary>
    Dictionary<string, string> GetMetadata();

    /// <summary>
    /// Get the current status message
    /// </summary>
    string GetStatusMessage();

    /// <summary>
    /// Event raised when display information changes
    /// </summary>
    event EventHandler? DisplayChanged;
}
