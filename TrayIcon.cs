using System.Windows.Forms;

namespace ICare;

public class TrayIcon : IDisposable
{
    private NotifyIcon _trayIcon;
    private CancellationTokenSource _token;

    public TrayIcon(CancellationTokenSource token)
    {
        _token = token;
        _trayIcon = new NotifyIcon();
        _trayIcon.Icon = System.Drawing.SystemIcons.Information; //TODO replace with the eye icon
        _trayIcon.Visible = true;
        _trayIcon.Text = "ICare";

        var menu = new ContextMenuStrip();
        menu.Items.Add("Quit", null, (s, e) => { token.Cancel(); Dispose(); });
        //TODO add a dashboard option
        _trayIcon.ContextMenuStrip = menu;
    }

    public void Dispose()
    {
        _trayIcon.Dispose();
    }
}