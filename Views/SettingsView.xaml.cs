using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using DataObject = System.Windows.DataObject;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class SettingsView : UserControl {
    private Config config;
    private Action restartTimer;
    private Action reloadHotkey;

    public SettingsView(Config _config, Action _restartTimer, Action _reloadHotkey) {
        config = _config;
        restartTimer = _restartTimer;
        reloadHotkey = _reloadHotkey;
        InitializeComponent();
        WorkBox.Text = $"{config.WorkSec / 60}";
        BreakBox.Text = $"{config.BreakSec}";
        RecalculateWarningIcon();
        HotkeyBox.Text = System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)config.HotkeyVK).ToString();
        AutoStartToggle.IsChecked = StartupManager.IsEnabled();
        BreakMessage.Text = config.BreakMessage;
        
        DataObject.AddPastingHandler(WorkBox, OnPaste);
        DataObject.AddPastingHandler(BreakBox, OnPaste);
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
        config.BreakMessage = BreakMessage.Text;
    }

    private void AutoStartToggle_Checked(object sender, RoutedEventArgs e) {
        StartupManager.Enable();
    }

    private void AutoStartToggle_Unchecked(object sender, RoutedEventArgs e) {
        StartupManager.Disable();
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
            BreakMessage.Text = config.BreakMessage;
        }
        
    }
}