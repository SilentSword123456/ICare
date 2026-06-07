using System.Windows;
using System.Windows.Interop;
using Microsoft.Toolkit.Uwp.Notifications;
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
        Keyboard keyboard = new Keyboard();
    
        var blackout = new BlackoutWindow(config, keyboard);
        var appTimer = new Timer(config);
        
        var helperWindow = new Window();
        helperWindow.Width = 0;
        helperWindow.Height = 0;
        helperWindow.WindowStyle = WindowStyle.None;
        helperWindow.ShowInTaskbar = false;
        helperWindow.Show();
        helperWindow.Hide();

        var handle = new WindowInteropHelper(helperWindow).Handle;
        keyboard.RegisterSkipHotkey(handle, () => appTimer.SkipNext = true);
        keyboard.StartListening(handle);
        
        ToastNotificationManagerCompat.OnActivated += args =>
        {
            if (args.Argument.Contains("skip"))
                appTimer.SkipNext = true;
        };
        
        _cts = new CancellationTokenSource();
        
        var tray = new TrayIcon(_cts);
        _ = appTimer.Start(_cts.Token, blackout.TriggerBreak);
    }
}