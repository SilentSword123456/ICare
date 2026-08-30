using System.Collections.ObjectModel;
using System.Windows;
using Button = System.Windows.Controls.Button;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class FoldersView : UserControl {
    public ObservableCollection<AppsFolder> Folders { get; set; } = new ();
    private Action<AppsFolder> OpenFolderView;

    public FoldersView(Action<AppsFolder> OpenFolderView, Config config) {
        this.OpenFolderView = OpenFolderView;
        Folders = config.AppsFolders;
        
        InitializeComponent();
        DataContext = this;
    }

    private void OnFolderClick(object sender, RoutedEventArgs e) {
        AppsFolder folder = (AppsFolder)((Button)sender).Tag;
        OpenFolderView(folder);
    }

    private void On_AddFolderButtonClick(object sender, RoutedEventArgs e) {
        int count = 0;
        foreach (var folder in Folders) {
            count++;
            if (folder.Name.CompareTo(("Folder " + count.ToString())) != 0)
                break;
        }
        
        Folders.Add(new AppsFolder(){Name=("Folder " + count.ToString()), TrackTime = true, SendNotification = true});
    }
}