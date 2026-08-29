using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json.Serialization;
using ICare.Models;

public class AppsFolder : INotifyPropertyChanged {
    private string _name;
    public string Name {
        get => _name;
        set { _name = value; OnPropertyChanged(nameof(Name)); }
    }
    
    private bool _trackTime;
    public bool TrackTime {
        get => _trackTime;
        set { _trackTime = value; OnPropertyChanged(nameof(TrackTime)); }
    }
    private bool _sendNotification;
    public bool SendNotification {
        get => _sendNotification;
        set { _sendNotification = value; OnPropertyChanged(nameof(SendNotification)); }
    }

    private bool _showOverlayWarning;
    public bool ShowOverlayWarning {
        get => _showOverlayWarning;
        set { _showOverlayWarning = value; OnPropertyChanged(nameof(ShowOverlayWarning)); }
    }
    
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public ObservableCollection<InstalledApp> Apps { get; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}