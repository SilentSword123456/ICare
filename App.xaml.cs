using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Toolkit.Uwp.Notifications;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Threading;
using ICare.Models;
using MaterialDesignThemes.Wpf;
using Velopack;
using Velopack.Sources;
using Velopack.Locators;

namespace ICare;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    private CancellationTokenSource _cts;
    private Dashboard dashboard;
    private Config config;
    private Keyboard keyboard;
    private Timer appTimer;
    private SessionMonitor sessionMonitor;
    private Mutex? _mutex;
    private PaletteHelper paletteHelper;
    private MaterialDesignThemes.Wpf.Theme theme;
    private ResourceDictionary currentThemeDictionary;
    private UpdateManager updateManager;
    
    public bool IsPortable => updateManager.IsPortable;
    public SemanticVersion? Version => updateManager.CurrentVersion;

    public event Action<UpdateState>? UpdateStateChanged;
    private UpdateState _updateState;
    public UpdateState UpdateState {
        get => _updateState;
        set {
            _updateState = value;
            UpdateStateChanged?.Invoke(value);
        }
    }

    public UpdateInfo? PendingUpdate;
    
    
    [STAThread]
    public static void Main(string[] args) {
        VelopackApp.Build().Run();
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
    
    public App() {
        #if DEBUG
            var testLocator = new TestVelopackLocator(
                appId: "ICare",
                version: "1.0.0",
                packagesDir: Path.Combine(Path.GetTempPath(), "ICareTestPackages"));

            updateManager = new UpdateManager(
                new SimpleFileSource(new DirectoryInfo(@"C:\Users\SilentSword\ICareTestUpdates")),
                null,
                testLocator);
        #else
            updateManager = new UpdateManager(new GithubSource("https://github.com/SilentSword123456/ICare", null, false));
        #endif
        
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        SentrySdk.Init(o =>
        {
            o.Dsn = "https://d745c29ea1a403c15ce7348fd13a4c6c@o4511711865536512.ingest.de.sentry.io/4511711891292240";
            o.Release = $"ICare@{Version}";
                
        #if DEBUG
            o.Environment = "development";
            o.Debug = true;
            o.TracesSampleRate = 1.0;
        #else
            o.Environment = "production";
            o.Debug = false;
        #endif
        });
    }
    
    void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) => SentrySdk.CaptureException(e.Exception);

    protected override void OnStartup(StartupEventArgs e) {
        _mutex = new Mutex(true, AppInfo.Name, out bool isNew);
        if (!isNew) {
            MessageBox.Show($"{AppInfo.Name} is already running. Check the system tray.");
            Shutdown();
            return;
        }

        base.OnStartup(e);
        config = new Config();
        config.Load();
        keyboard = new Keyboard(config);
        appTimer = new Timer(config, keyboard);
        dashboard = new Dashboard(config, appTimer, keyboard);
        paletteHelper = new PaletteHelper();
        theme = paletteHelper.GetTheme();

        var helperWindow = new Window();
        helperWindow.Width = 0;
        helperWindow.Height = 0;
        helperWindow.WindowStyle = WindowStyle.None;
        helperWindow.ShowInTaskbar = false;
        helperWindow.Show();
        helperWindow.Hide();

        var handle = new WindowInteropHelper(helperWindow).Handle;
        keyboard.RegisterSkipHotkey(handle, appTimer.SkipNextBreak);
        keyboard.StartListening(handle);

        ToastNotificationManagerCompat.OnActivated += args => {
            var toastArgs = ToastArguments.Parse(args.Argument);
            switch (toastArgs["action"]) {
                case "skip":
                    appTimer.SkipNextBreak();
                    break;
                case "snooze5":
                    appTimer.SnoozeBreak(5);
                    break;
                case "snooze10":
                    appTimer.SnoozeBreak(10);
                    break;
            }
        };
        
        SetTheme(config.Theme);
        
        _cts = new CancellationTokenSource();

        var tray = new TrayIcon(CloseApp, dashboard);
        _ = appTimer.Start();
        
        dashboard.Open();
        UpdateTitleBarTheme(dashboard, config.Theme ==  Theme.Dark);
        
        Application.Current.MainWindow = dashboard;
        sessionMonitor = new SessionMonitor(Application.Current.MainWindow, appTimer);

        _ = CheckForUpdates();
    }

    public void CloseApp() {
        sessionMonitor.Stop();
        _cts.Cancel();
    }

    public void SetTheme(Theme targetTheme) {
        if (targetTheme == Theme.Light)
            theme.SetBaseTheme(BaseTheme.Light);
        else
            theme.SetBaseTheme(BaseTheme.Dark);
        
        paletteHelper.SetTheme(theme);
        ApplyThemeDictionary(targetTheme);
        
        if (dashboard != null)
            UpdateTitleBarTheme(dashboard, config.Theme ==  Theme.Dark);
    }
    
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private void UpdateTitleBarTheme(Window? window, bool isDark) {
        if (window == null) 
            return;
        var hwnd = new WindowInteropHelper(window).Handle;
        int darkMode = isDark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, 20, ref darkMode, sizeof(int));
    }
    
    private void ApplyThemeDictionary(Theme targetTheme) {
        var newDictionary = new ResourceDictionary();
        newDictionary.Source = new Uri(targetTheme == Theme.Light ? "pack://application:,,,/Themes/Light.xaml" : "pack://application:,,,/Themes/Dark.xaml", UriKind.Absolute);

        if (currentThemeDictionary != null)
            Application.Current.Resources.MergedDictionaries.Remove(currentThemeDictionary);

        Application.Current.Resources.MergedDictionaries.Add(newDictionary);

        currentThemeDictionary = newDictionary;
    }

    public async Task CheckForUpdates() {
        UpdateState = UpdateState.Checking;
        PendingUpdate = await updateManager.CheckForUpdatesAsync();
        UpdateState = PendingUpdate != null ? UpdateState.Available : UpdateState.NotFound;
    }
    
    public async Task DownloadUpdate() {
        if (PendingUpdate == null)
            return;
        UpdateState = UpdateState.Downloading;
        await updateManager.DownloadUpdatesAsync(PendingUpdate);
        UpdateState = UpdateState.ReadyToInstall;
    }

    public void ApplyUpdates() {
        if  (PendingUpdate == null)
            return;
        updateManager.ApplyUpdatesAndRestart(PendingUpdate);
    }
        
}