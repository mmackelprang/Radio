using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace RadioConsole.ViewModels;

public partial class HistoryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<HistoryItem> _historyItems = new();

    public HistoryViewModel()
    {
        // Load history items
        LoadHistory();
    }

    private void LoadHistory()
    {
        // TODO: Load from storage
        // For now, add some placeholder items
        HistoryItems.Add(new HistoryItem 
        { 
            Title = "FM 101.5 - Classic Rock",
            Timestamp = DateTime.Now.AddHours(-2),
            Source = "Radio"
        });
        HistoryItems.Add(new HistoryItem 
        { 
            Title = "Sample Track - Sample Artist",
            Timestamp = DateTime.Now.AddHours(-5),
            Source = "Spotify"
        });
    }
}

public class HistoryItem
{
    public string Title { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
}
