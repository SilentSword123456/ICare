using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ICare;

public partial class BlackoutWindow : Window {
    private static BitmapImage background;
    private static Theme? theme;
    private Config config;
    
    public BlackoutWindow(Config config) {
        this.config = config;
        InitializeComponent();
        ApplyBackgroundForTheme();
    }

    private void VerifyBackground() {
        if (theme != null && theme == config.Theme)
            return;

        if (config.Theme == Theme.Light) {
            background = new BitmapImage(new Uri($"pack://application:,,,/{AppInfo.Name};component/Assets/background.png"));
            background.Freeze();
            theme = Theme.Light;
            return;
        }

        background = new BitmapImage(new Uri($"pack://application:,,,/{AppInfo.Name};component/Assets/background-dark.png"));
        background.Freeze();
        theme = Theme.Dark;
        return;
    }
    
    private void ApplyBackgroundForTheme() {
        VerifyBackground();
        BackgroundImage.ImageSource = background;
    }

    static async public Task TriggerBreak(Keyboard keyboard, Config config) {
        keyboard.StartBlocking();
        config.Load();

        var windows = System.Windows.Forms.Screen.AllScreens
            .Select(screen => {
                var window = new BlackoutWindow(config);
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