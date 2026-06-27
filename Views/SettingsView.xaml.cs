using System.Windows.Controls;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class SettingsView : UserControl
{
    private Config config;
    private Action restartTimer;
    private Action reloadHotkey;

    public SettingsView(Config _config, Action _restartTimer, Action _reloadHotkey)
    {
        config = _config;
        restartTimer = _restartTimer;
        reloadHotkey =  _reloadHotkey;
        InitializeComponent();
        WorkBox.Text = $"{config.WorkSec / 60}";
        BreakBox.Text = $"{config.BreakSec}";
        HotkeyBox.Text = System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)config.HotkeyVK).ToString();
        AutoStartToggle.IsChecked = StartupManager.IsEnabled();
        BreakMessage.Text = config.BreakMessage;
    }

    private void WorkBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(WorkBox.Text, out int val) && val > 0)
        {
            var newSec = val * 60;
            if (config.WorkSec == newSec) 
                return;
            config.WorkSec = newSec;
            restartTimer();
        }
    }

    private void BreakBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(BreakBox.Text, out int val) && val > 0)
        {
            config.BreakSec = val;
        }
    }

    private void HotkeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key;
        if (key < System.Windows.Input.Key.A || key > System.Windows.Input.Key.Z)
            return;

        config.HotkeyVK = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        HotkeyBox.Text = key.ToString();
        reloadHotkey();
    }
    
    private void BreakMessage_TextChanged(object sender, TextChangedEventArgs e)
    {
        config.BreakMessage = BreakMessage.Text;
    }
    
    private void AutoStartToggle_Checked(object sender, RoutedEventArgs e)
    {
        StartupManager.Enable();
    }

    private void AutoStartToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        StartupManager.Disable();
    }
}