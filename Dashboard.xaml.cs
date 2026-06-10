using System.Windows;
using ICare.Views;

namespace ICare;

public partial class Dashboard : Window
{
    private Config config;
    private Timer timer;

    public Dashboard(Config config, Timer timer)
    {
        this.config = config;
        this.timer = timer;
        InitializeComponent();
        ContentArea.Content = new OverviewView(this.config, this.timer);
    }

    private void NavigateTo(object view)
    {
        ContentArea.Opacity = 0;
        ContentArea.Content = view;

        var fade = new System.Windows.Media.Animation.DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180));
        ContentArea.BeginAnimation(OpacityProperty, fade);
    }

    private void NavOverview_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new OverviewView(config, timer));
        NavOverview.Style = (Style)FindResource("NavButtonActive");
        NavSettings.Style = (Style)FindResource("NavButton");
    }

    private void NavSettings_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new SettingsView(config, timer.Restart));
        NavSettings.Style = (Style)FindResource("NavButtonActive");
        NavOverview.Style = (Style)FindResource("NavButton");
    }
    
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
    
    
}