namespace RadioConsole.Api.Models;

/// <summary>
/// Represents metadata about currently playing audio
/// </summary>
public class AudioMetadata
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? Station { get; set; }
    public string? Genre { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan? Position { get; set; }
    public string? AlbumArtUrl { get; set; }
    public Dictionary<string, string> AdditionalInfo { get; set; } = new();
}

/// <summary>
/// Represents a history entry
/// </summary>
public class HistoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string InputSource { get; set; } = string.Empty;
    public AudioMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Represents a favorite item
/// </summary>
public class FavoriteEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string InputSource { get; set; } = string.Empty;
    public string InputConfiguration { get; set; } = string.Empty;
    public AudioMetadata? LastKnownMetadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastAccessedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Represents the current playback state
/// </summary>
public class PlaybackState
{
    public bool IsPlaying { get; set; }
    public string? CurrentInputId { get; set; }
    public string? CurrentOutputId { get; set; }
    public double Volume { get; set; } = 0.5;
    public AudioMetadata? CurrentMetadata { get; set; }
}

/// <summary>
/// Request model for starting playback
/// </summary>
public class StartPlaybackRequest
{
    public string InputId { get; set; } = string.Empty;
    public string OutputId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for setting volume
/// </summary>
public class VolumeRequest
{
    public int Volume { get; set; }
}
