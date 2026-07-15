using System.Windows;
using System.Windows.Shapes;
using ICare.Views;

namespace ICare;

public partial class Dashboard : Window {
    private Config config;
    private Timer timer;
    private Keyboard keyboard;

    public Dashboard(Config config, Timer timer, Keyboard keyboard) {
        this.config = config;
        this.timer = timer;
        this.keyboard = keyboard;
        Title = AppInfo.Name;
        InitializeComponent();
        ContentArea.Content = new OverviewView(this.config, this.timer);
    }

    private void NavigateTo(object view) {
        ContentArea.Opacity = 0;
        ContentArea.Content = view;

        var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        ContentArea.BeginAnimation(OpacityProperty, fade);
    }

    private void NavOverview_Click(object sender, RoutedEventArgs e) {
        if (ContentArea.Content is OverviewView overviewView)
            return;
        NavigateTo(new OverviewView(config, timer));
        NavOverview.Style = (Style)FindResource("NavButtonActive");
        NavSettings.Style = (Style)FindResource("NavButton");
    }

    private void NavSettings_Click(object sender, RoutedEventArgs e) {
        if (ContentArea.Content is SettingsView settingsView)
            return;
        NavigateTo(new SettingsView(config, timer.Restart, keyboard.ReloadHotkey));
        NavSettings.Style = (Style)FindResource("NavButtonActive");
        NavOverview.Style = (Style)FindResource("NavButton");
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
        e.Cancel = true;
        Hide();
    }

    

    public void Open() {
        if (IsVisible)
            Activate();
        else
            Show();
    }
}