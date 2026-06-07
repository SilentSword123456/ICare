using System.Windows;
using System.Windows.Interop;
using Microsoft.Toolkit.Uwp.Notifications;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ICare;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private CancellationTokenSource _cts;
    private Dashboard dashboard;
    private Config config;
    private Keyboard keyboard;
    private BlackoutWindow blackout;
    private Timer appTimer;
    private Mutex? _mutex;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, "ICare", out bool isNew);
        if (!isNew)
        {
            MessageBox.Show("ICare is already running. Check the system tray.");
            Shutdown();
            return;
        }

        
        base.OnStartup(e);
        config = new Config();
        config.Load();
        keyboard = new Keyboard();
        blackout = new BlackoutWindow(config, keyboard);
        appTimer = new Timer(config, blackout.TriggerBreak);
        dashboard = new Dashboard(config, appTimer);
        
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
        
        var tray = new TrayIcon(_cts, dashboard);
        _ = appTimer.Start();
    }
}