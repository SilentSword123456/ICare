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
using ICare.Utilities;
using Sentry.Extensibility;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class AppsView : UserControl {
    private Process[] AllProcesses;
    public ObservableCollection<InstalledApp> Apps { get; set; } = new();
    private ListCollectionView view;
    private AppsFolder folder;
    public AppsView(AppsFolder folder) {
        InitializeComponent();
        DataContext = this;
        this.folder = folder;
        
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
                found.Add(new InstalledApp { Name = Path.GetFileNameWithoutExtension(file), ExePath = shortcut.TargetPath, Icon = IconHelper.GetIconForExe(shortcut.TargetPath) });
                Dispatcher.Invoke(() =>
                    Apps.Add(new InstalledApp {
                        Name = Path.GetFileNameWithoutExtension(file), ExePath = shortcut.TargetPath,
                        Icon = IconHelper.GetIconForExe(shortcut.TargetPath)
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

    private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        if (e.AddedItems.Count == 0)
            return;
        
        var selected = (InstalledApp)e.AddedItems[0];
        
        bool alreadyInFolder = folder.Apps.Any(app => app.ExePath == selected.ExePath);
        if (!alreadyInFolder)
            folder.Apps.Add(selected);

        ListBox.SelectedItem = null;
    }
}