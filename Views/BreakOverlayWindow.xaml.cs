using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ICare.Views;

public partial class BreakOverlayWindow : Window {
    private DispatcherTimer breakTask;
    private TimeSpan WarningDuration;
    private Config config;
    private TaskCompletionSource? tcs;
    
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_NOACTIVATE  = 0x08000000;
    private const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
    
    public BreakOverlayWindow(Config config) {
        this.config = config;
        
        breakTask = new ();
        breakTask.Interval = TimeSpan.FromSeconds(1);
        
        breakTask.Tick += (sender, e) => {
            WarningDuration -= TimeSpan.FromSeconds(1);
            
            this.Dispatcher.Invoke(() => CountdownText.Text = WarningDuration.TotalSeconds.ToString());
            if (WarningDuration == TimeSpan.FromSeconds(3))
                CountdownText.Foreground = System.Windows.Media.Brushes.Red;
            
            if(WarningDuration <= TimeSpan.Zero) {
                tcs.SetResult();
                breakTask.Stop();
            }
        };
        
        InitializeComponent();
        Loaded += (s, e) =>
        {
            Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
            Top = 40;
        };
    }
    
     protected override void OnSourceInitialized(EventArgs e) {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
     }

    private void UpdateHotkeyText() {
        SkipKeyRun.Text = "Control+Shift+" + KeyInterop.KeyFromVirtualKey((int) config.HotkeyVk).ToString();
    }

    public async Task StartBreakWarning(TimeSpan warningDuration, CancellationToken cts, String WarningMessage) {
        this.WarningDuration = warningDuration;
        CountdownText.Text = warningDuration.TotalSeconds.ToString();
        UpdateHotkeyText();
        CountdownText.Foreground = System.Windows.Media.Brushes.White;
        
        tcs = new TaskCompletionSource();
        
        var internalCts = new CancellationTokenSource();
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, cts);
        var cancellationTokenTask = Task.Delay(Timeout.Infinite, linkedTokenSource.Token);
        
        BreakOverlayMessage.Text = WarningMessage;
        
        Open();
        breakTask.Stop();
        breakTask.Start();
        
        var fired = await Task.WhenAny([tcs.Task, cancellationTokenTask]);
        
        if (fired == tcs.Task) {
            internalCts.Cancel();
            tcs.Task.Dispose();
        }
        else
            breakTask.Stop();
        
        internalCts.Dispose();
        cancellationTokenTask.Dispose();
        linkedTokenSource.Cancel();
        linkedTokenSource.Dispose();
        
        Hide();
    }

    public void StopBreakWarning() {
        if (tcs == null) 
            return;
        if (!tcs.Task.IsCompleted)
            tcs.SetResult();
        breakTask.Stop();
    }
    
    private void Open() {
        if (WindowState == WindowState.Minimized)
            WindowState = WindowState.Normal;
        
        if (IsVisible)
            Activate();
        else
            Show();
    }
}