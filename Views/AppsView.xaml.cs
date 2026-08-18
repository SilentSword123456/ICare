using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ICare.Models;
using Sentry.Extensibility;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class AppsView : UserControl {
    private Process[] AllProcesses;
    public ObservableCollection<InstalledApp> Apps { get; set; } = new();
    private ListCollectionView view;
    public AppsView() {
        InitializeComponent();
        DataContext = this;
        
        Task.Run(ListApps);
        
        view = (ListCollectionView)CollectionViewSource.GetDefaultView(Apps);
        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        view.LiveSortingProperties.Add("Name");
        view.IsLiveSorting = true;
        
        view.Filter = item =>
        {
            var i = (InstalledApp)item;
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
                return true;

            return i.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
        };

        ListBox.ItemsSource = view;
    }
    
    public void ListApps() {
        AllProcesses = Process.GetProcesses();
        
        Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
        dynamic shell = Activator.CreateInstance(shellType);

        string allUsersStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
        string userStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
        
        var found = new List<InstalledApp>();
        
        foreach (string file in Directory.EnumerateFiles(userStartMenu, "*.lnk", SearchOption.AllDirectories)
                     .Concat(Directory.EnumerateFiles(allUsersStartMenu, "*.lnk", SearchOption.AllDirectories)))
            try {
                dynamic shortcut = shell.CreateShortcut(file);
                string targetPath = (string)shortcut.TargetPath;
                Debug.WriteLine(Path.GetFileNameWithoutExtension(file) + " -> " + targetPath);
                found.Add(new InstalledApp { Name = Path.GetFileNameWithoutExtension(file), ExePath = shortcut.TargetPath, Icon = GetIconForExe(shortcut.TargetPath) });
                Dispatcher.Invoke(() =>
                    Apps.Add(new InstalledApp {
                        Name = Path.GetFileNameWithoutExtension(file), ExePath = shortcut.TargetPath,
                        Icon = GetIconForExe(shortcut.TargetPath)
                    }));
            }
            catch (Exception e) {
                SentrySdk.CaptureException(e);
                Console.WriteLine(e.Message);
            }
    }

    private void SearchBoxTextChanged(object sender, TextChangedEventArgs e) {
        if (view == null)
            return;
        view.Refresh();
    }
    
    private ImageSource? GetIconForExe(string exePath) {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return null;

        using Icon? icon = Icon.ExtractAssociatedIcon(exePath);
        if (icon == null)
            return null;

        ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
            icon.Handle,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
        imageSource.Freeze();

        return imageSource;
    }
}