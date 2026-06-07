using System.Windows;

namespace ICare;

public partial class BlackoutWindow : Window
{
    private Config _config;
    
    public BlackoutWindow(Config config)
    {
        _config = config;
        InitializeComponent();
    }

    async public void TriggerBreak()
    {
        this.Show();
        var endBreakTime = DateTime.Now.AddSeconds(_config.BreakSec);
        while (DateTime.Now < endBreakTime) {
            await Task.Delay(1000);
            CountdownText.Text = ((int)(endBreakTime - DateTime.Now).TotalSeconds).ToString();
        }
        this.Hide();
    }
}