using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ICare.Views;

public partial class BreakOverlayWindow : Window {
    private DispatcherTimer breakTask;
    private TimeSpan WarningDuration;
    private Config config;
    private TaskCompletionSource? tcs;
    
    public BreakOverlayWindow(Config config) {
        this.config = config;
        
        breakTask = new ();
        breakTask.Interval = TimeSpan.FromSeconds(1);
        
        breakTask.Tick += (sender, e) => {
            WarningDuration -= TimeSpan.FromSeconds(1);
            
            this.Dispatcher.Invoke(() => CountdownText.Text = WarningDuration.TotalSeconds.ToString());
            if(WarningDuration <= TimeSpan.Zero) {
                tcs.SetResult();
                breakTask.Stop();
            }
        };
        
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
        Left = 0;
        Top = 0;
        
        InitializeComponent();
    }

    private void UpdateHotkeyText() {
        SkipKeyRun.Text = "Control+Shift+" + KeyInterop.KeyFromVirtualKey((int) config.HotkeyVk).ToString();
    }

    public async Task StartBreakWarning(TimeSpan warningDuration, CancellationToken cts) {
        this.WarningDuration = warningDuration;
        CountdownText.Text = warningDuration.TotalSeconds.ToString();
        UpdateHotkeyText();
        
        tcs = new TaskCompletionSource();
        
        var internalCts = new CancellationTokenSource();
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCts.Token, cts);
        var cancellationTokenTask = Task.Delay(Timeout.Infinite, linkedTokenSource.Token);
        
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