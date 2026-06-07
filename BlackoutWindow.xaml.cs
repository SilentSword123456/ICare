using System.Windows;

namespace ICare;

public partial class BlackoutWindow : Window
{
    private Config config;
    private Keyboard keyboard;
    
    public BlackoutWindow(Config _config, Keyboard _keyboard)
    {
        config = _config;
        keyboard = _keyboard;
        InitializeComponent();
    }

    async public Task TriggerBreak()
    {
        keyboard.StartBlocking();
        this.Show();
        var endBreakTime = DateTime.Now.AddSeconds(config.BreakSec);
        while (DateTime.Now < endBreakTime) {
            await Task.Delay(1000);
            CountdownText.Text = ((int)(endBreakTime - DateTime.Now).TotalSeconds).ToString();
        }
        this.Hide();
        keyboard.StopBlocking();
    }
}