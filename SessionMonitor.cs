using Microsoft.Win32;

namespace ICare;

public class SessionMonitor {
    private Timer timer;

    public SessionMonitor(Timer timer) {
        this.timer = timer;
        SystemEvents.SessionSwitch += OnSessionSwitch;
    }

    public void Stop() => SystemEvents.SessionSwitch -= OnSessionSwitch;

    private void OnSessionSwitch(object sender, SessionSwitchEventArgs e) {
        if (e.Reason == SessionSwitchReason.SessionLock)
            timer.Pause();
        else if (e.Reason == SessionSwitchReason.SessionUnlock)
            timer.Resume();
    }
}