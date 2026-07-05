using System.Windows;

namespace ICare;

public partial class BlackoutWindow : Window {
    private Config config;
    private Keyboard keyboard;

    public BlackoutWindow(Config _config, Keyboard _keyboard) {
        config = _config;
        keyboard = _keyboard;
        InitializeComponent();
    }

    async public Task TriggerBreak() {
        keyboard.StartBlocking();
        config.Load();
        MessageText.Text = config.BreakMessage;

        var windows = System.Windows.Forms.Screen.AllScreens
            .Select(screen => {
                var window = new BlackoutWindow(config, keyboard);
                window.Left = screen.Bounds.Left;
                window.Top = screen.Bounds.Top;
                window.Width = screen.Bounds.Width;
                window.Height = screen.Bounds.Height;
                window.MessageText.Text = screen.Primary ? config.BreakMessage : "";
                window.CountdownText.Text = "";
                window.Show();
                return window;
            }).ToList();

        var endBreakTime = DateTime.Now.AddSeconds(config.BreakSec);
        while (DateTime.Now < endBreakTime) {
            var remaining = ((int)(endBreakTime - DateTime.Now).TotalSeconds + 1).ToString();

            windows[0].CountdownText.Text = remaining;

            await Task.Delay(1000);
        }

        foreach (var window in windows)
            window.Close();

        keyboard.StopBlocking();
        config.BreakStatSec += config.BreakSec;
    }
}