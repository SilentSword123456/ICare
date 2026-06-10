using System.Windows.Controls;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace ICare.Views;

public partial class SettingsView : UserControl
{
    private Config config;
    private Action restartTimer;

    public SettingsView(Config _config, Action _restartTimer)
    {
        config = _config;
        restartTimer = _restartTimer;
        InitializeComponent();
        WorkBox.Text = $"{config.WorkSec / 60}";
        BreakBox.Text = $"{config.BreakSec}";
        HotkeyBox.Text = config.Hotkey;
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
            config.Save();
            restartTimer();
        }
    }

    private void BreakBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(BreakBox.Text, out int val) && val > 0)
        {
            config.BreakSec = val;
            config.Save();
        }
    }

    private void HotkeyBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        config.Hotkey = HotkeyBox.Text;
        config.Save();
    }
    
    private void BreakMessage_TextChanged(object sender, TextChangedEventArgs e)
    {
        config.BreakMessage = BreakMessage.Text;
        config.Save();
    }
}