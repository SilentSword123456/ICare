using System.Net.Mime;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using DataObject = System.Windows.DataObject;
using UserControl = System.Windows.Controls.UserControl;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using ABI.Windows.Foundation.Diagnostics;
using ICare.Models;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace ICare.Views;

public partial class SettingsView : UserControl {
    private Config config;
    private Action restartTimer;
    private Action reloadHotkey;

    public SettingsView(Config config, Action restartTimer, Action reloadHotkey) {
        this.config = config;
        this.restartTimer = restartTimer;
        this.reloadHotkey = reloadHotkey;
        InitializeComponent();
        WorkBox.Text = $"{config.WorkSec / 60}";
        BreakBox.Text = $"{config.BreakSec}";
        RecalculateWarningIcon();
        HotkeyBox.Text = System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)config.HotkeyVK).ToString();
        AutoStartToggle.IsChecked = StartupManager.IsEnabled();
        //BreakMessage.Text = config.BreakMessage;
        ThemePillButton.IsChecked = config.Theme == Theme.Dark;
        if (((App)Application.Current).IsPortable)
            UpdateButton.Visibility = Visibility.Collapsed;
        UpdateUpdateState(((App)Application.Current).UpdateState);
        
        DataObject.AddPastingHandler(WorkBox, OnPaste);
        DataObject.AddPastingHandler(BreakBox, OnPaste);
        
        ((App)Application.Current).UpdateStateChanged += UpdateUpdateState;
    }

    private void WorkBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (int.TryParse(WorkBox.Text, out int val) && val > 0) {
            var newSec = val * 60;
            if (config.WorkSec == newSec)
                return;
            config.WorkSec = newSec;
            restartTimer();
            RecalculateWarningIcon();
        }
    }

    private void BreakBox_TextChanged(object sender, TextChangedEventArgs e) {
        if (int.TryParse(BreakBox.Text, out int val) && val > 0) {
            config.BreakSec = val;
            RecalculateWarningIcon();
        }
    }
    
    private void OnPaste(object sender, DataObjectPastingEventArgs e) {
        if (e.DataObject.GetDataPresent(typeof(string))) {
            string text = (string)e.DataObject.GetData(typeof(string));
            
            if (!text.All(char.IsDigit)) 
                e.CancelCommand();
        } 
        else {
            e.CancelCommand();
        }
    }
    
    private void PreviewTextInputAcceptOnlyNumbers(object sender, TextCompositionEventArgs e) {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void HotkeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
        e.Handled = true;
        var key = e.Key;
        if (key < System.Windows.Input.Key.A || key > System.Windows.Input.Key.Z)
            return;

        config.HotkeyVK = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        HotkeyBox.Text = key.ToString();
        reloadHotkey();
    }

    private void BreakMessage_TextChanged(object sender, TextChangedEventArgs e) {
        //config.BreakMessage = BreakMessage.Text;
    }

    private void AutoStartToggle_Checked(object sender, RoutedEventArgs e) {
        StartupManager.Enable();
        UpdateToggleVisual(AutoStartToggle);
    }

    private void AutoStartToggle_Unchecked(object sender, RoutedEventArgs e) {
        StartupManager.Disable();
        UpdateToggleVisual(AutoStartToggle);
    }

    private void UpdateToggleVisual(ToggleButton toggle) {
        toggle.ApplyTemplate();
        var track = (Border)toggle.Template.FindName("Track", toggle);
        var thumb = (Ellipse)toggle.Template.FindName("Thumb", toggle);

        bool isChecked = toggle.IsChecked == true;

        Color targetColor;
        if (isChecked)
            targetColor = (Color)ColorConverter.ConvertFromString("#006400");
        else {
            var themeBrush = (SolidColorBrush)System.Windows.Application.Current.Resources["ToggleTrackBrush"];
            targetColor = themeBrush.Color;
        }

        var colorAnimation = new ColorAnimation {
            To = targetColor,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        
        if (track.Background.IsFrozen)
            track.Background = track.Background.Clone();
        
        track.Background.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        
        Thickness targetThickness = isChecked ? new Thickness(23, 0, 0, 0) :  new Thickness(3, 0, 0, 0);
        
        var thicknessAnimation = new ThicknessAnimation {
            To = targetThickness,
            Duration = TimeSpan.FromSeconds(0.2)
        };
        thumb.BeginAnimation(MarginProperty, thicknessAnimation);
    }
    
    public void RefreshToggleVisual() {
        UpdateToggleVisual(AutoStartToggle);
    }

    private void RecalculateWarningIcon() {
        BreakWarningIcon.Visibility =
            config.WorkSec / 60 <= config.BreakSec ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void ResetAppSettings_Click(object sender, RoutedEventArgs e) {
        var result = await DialogHost.Show(new ConfirmResetDialog(), "RootDialog");

        if (result is ResetDialogResult.Confirm) {
            config.ResetToDefault();
            restartTimer();
            WorkBox.Text = $"{config.WorkSec / 60}";
            BreakBox.Text = $"{config.BreakSec}";
            RecalculateWarningIcon();
            StartupManager.Disable();
            AutoStartToggle.IsChecked = false;
            HotkeyBox.Text = System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)config.HotkeyVK).ToString();
            //BreakMessage.Text = config.BreakMessage;
            ThemePillButton.IsChecked = config.Theme == Theme.Dark;

        }
        
    }
    
    private void SwitchTheme(object sender, RoutedEventArgs e) {
        config.Theme = config.Theme == Theme.Light ? Theme.Dark : Theme.Light;
        
        var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (s, args) => {
            ((App)System.Windows.Application.Current).SetTheme(config.Theme);
            
            RefreshToggleVisual();

            var fadeIn = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            ThemePillButton.BeginAnimation(OpacityProperty, fadeIn);
        };

        ThemePillButton.BeginAnimation(OpacityProperty, fadeOut);
    }
    
    private async void UpdateAppClick(object sender, RoutedEventArgs e) {
        switch (((App)Application.Current).UpdateState){
            case UpdateState.Idle: {
                await ((App)Application.Current).CheckForUpdates();
                break;
            }
            case UpdateState.NotFound: {
                await ((App)Application.Current).CheckForUpdates();
                break;
            }
            case UpdateState.Available: {
                await ((App)Application.Current).DownloadUpdate();
                break;
            }
            case UpdateState.ReadyToInstall:{
                var result = await DialogHost.Show(new ConfirmUpdate(), "RootDialog");

                if (result is ResetDialogResult.Confirm) 
                    ((App)Application.Current).ApplyUpdates();

                break;
            }
        }
    }

    private void UpdateUpdateState(UpdateState state) {
        switch (state) {
            case UpdateState.Idle: UpdateButtonText.Text = "[Check for updates]"; break;
            case UpdateState.NotFound: UpdateButtonText.Text = "[No updates found, click to check again]"; break;
            case UpdateState.Available: UpdateButtonText.Text = "[Found new version, click to install]"; break;
            case UpdateState.Checking: UpdateButtonText.Text = "[Checking...]"; break;
            case UpdateState.ReadyToInstall: UpdateButtonText.Text = "[Download finished, click to restart]"; break;
        }
    }
}