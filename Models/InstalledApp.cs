using System.ComponentModel;
using System.Windows.Media;

namespace ICare.Models;

public class InstalledApp : INotifyPropertyChanged{
    
    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    public string ExePath { get; set; }
    public ImageSource Icon { get; set; }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}