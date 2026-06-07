using System.Windows.Forms;

namespace ICare;

public class TrayIcon : IDisposable
{
    private NotifyIcon trayIcon;
    private CancellationTokenSource token;
    private Dashboard dashboard;

    public TrayIcon(CancellationTokenSource _token, Dashboard _dashboard) {
        token = _token;
        dashboard = _dashboard;
        trayIcon = new NotifyIcon();
        using var stream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("ICare.eye.ico");
        trayIcon.Icon = new System.Drawing.Icon(stream!);
        trayIcon.Visible = true;
        trayIcon.Text = "ICare";

        var menu = new ContextMenuStrip();
        menu.Items.Add("Quit", null, (s, e) => { CloseApp(); });
        trayIcon.DoubleClick += (s, e) => OpenDashboard();
        trayIcon.Text = "ICare";
        trayIcon.ContextMenuStrip = menu;
    }

    private void CloseApp()
    {
        token.Cancel();
        System.Windows.Application.Current.Dispatcher.Invoke(() => {
            System.Windows.Application.Current.Shutdown();
        });
    }

    private void OpenDashboard()
    {
        if (dashboard.IsVisible)
            dashboard.Activate();
        else
            dashboard.Show();
    }

    public void Dispose()
    {
        trayIcon.Dispose();
    }
}