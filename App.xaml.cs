using System.Windows;
using Application = System.Windows.Application;

namespace ICare;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private CancellationTokenSource _cts;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Config config = new Config();
        config.Load();
    
        var blackout = new BlackoutWindow(config);
        var appTimer = new Timer(config);
        _cts = new CancellationTokenSource();
        
        var tray = new TrayIcon(_cts);
        _ = appTimer.Start(_cts.Token, blackout.TriggerBreak);
    }
}