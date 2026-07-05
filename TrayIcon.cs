using System.Windows.Forms;

namespace ICare;

public class TrayIcon : IDisposable {
    private NotifyIcon trayIcon;
    private Dashboard dashboard;
    private Action CloseApp;

    public TrayIcon(Action CloseApp, Dashboard dashboard) {
        this.CloseApp = CloseApp;
        this.dashboard = dashboard;
        trayIcon = new NotifyIcon();
        using var stream = System.Reflection.Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("ICare.eye.ico");
        trayIcon.Icon = new System.Drawing.Icon(stream!);
        trayIcon.Visible = true;
        trayIcon.Text = AppInfo.Name;

        var menu = new ContextMenuStrip();
        menu.Items.Add("Quit", null, (s, e) => { Exit(); });
        trayIcon.MouseClick += (s, e) => {
            if (e.Button == MouseButtons.Left)
                this.dashboard.Open();
        };
        trayIcon.Text = AppInfo.Name;
        trayIcon.ContextMenuStrip = menu;
    }

    private void Exit() {
        System.Windows.Application.Current.Dispatcher.Invoke(() => { System.Windows.Application.Current.Shutdown(); });
        CloseApp();
    }

    public void Dispose() {
        trayIcon.Dispose();
    }
}