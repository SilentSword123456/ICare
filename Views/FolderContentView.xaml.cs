using System.Windows;
using ICare.Models;
using UserControl = System.Windows.Controls.UserControl;
using Button = System.Windows.Controls.Button;

namespace ICare.Views;

public partial class FolderContentView : UserControl {
    private AppsFolder folder;
    private bool isInitializing = true;
    
    public FolderContentView(AppsFolder folder) {
        this.folder = folder;
        InitializeComponent();
        DataContext = folder;
        
        FolderNameText.Text = folder.Name;
        AppCountText.Text = folder.Apps.Count.ToString() + (folder.Apps.Count == 1 ? " app" : " apps");
        TrackTimeCheckbox.IsChecked = folder.TrackTime;
        RuleSendNotificationCheckbox.IsChecked = folder.SendNotification;
        RuleShowOverlayWarningCheckbox.IsChecked = folder.ShowOverlayWarning;
        RuleSendNotificationCheckbox.IsEnabled = folder.TrackTime;
        RuleShowOverlayWarningCheckbox.IsEnabled = folder.TrackTime;
        AddAppsButton.Visibility = Visibility.Visible;
        CloseAddAppsButton.Visibility = Visibility.Collapsed;
        
        AppsListControl.ItemsSource = folder.Apps;
        folder.Apps.CollectionChanged += (s, e) => 
            AppCountText.Text = folder.Apps.Count.ToString() + (folder.Apps.Count == 1 ? " app" : " apps");
        
        isInitializing = false;
    }

    private void RemoveApp_Click(object sender, RoutedEventArgs e) {
        var button = (Button)sender;
        var app = (InstalledApp)button.Tag;

        folder.Apps.Remove(app);
    }

    private void AddAppsButton_Click(object sender, RoutedEventArgs e) {
        if (AddAppsSlot.Content is AppsView appsView)
            return;
        
        AddAppsSlot.Opacity = 0;
        AddAppsSlot.Content = new AppsView(folder);

        var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        AddAppsSlot.BeginAnimation(OpacityProperty, fade);
        AddAppsButton.Visibility = Visibility.Collapsed;
        CloseAddAppsButton.Visibility = Visibility.Visible;
    }

    private void TrackTimeCheckbox_Pressed(object sender, RoutedEventArgs e) {
        if (isInitializing)
            return;
        folder.TrackTime = !folder.TrackTime;
        RuleSendNotificationCheckbox.IsEnabled = folder.TrackTime;
        RuleShowOverlayWarningCheckbox.IsEnabled = folder.TrackTime;

    }

    private void RuleSendNotificationCheckbox_OnCheckedChecked(object sender, RoutedEventArgs e) {
        if (isInitializing)
            return;
        folder.SendNotification = !folder.SendNotification;
    }

    private void RuleShowOverlayWarning_Checkbox_OnChecked(object sender, RoutedEventArgs e) {
        if (isInitializing)
            return;
        folder.ShowOverlayWarning = !folder.ShowOverlayWarning;
    }
    
    private void CloseAddApps_Click(object sender, RoutedEventArgs e) {
        AddAppsSlot.Content = null;
        AddAppsButton.Visibility = Visibility.Visible;
        CloseAddAppsButton.Visibility = Visibility.Collapsed;
    }
}